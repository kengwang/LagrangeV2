using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FastEndpoints;
using FastEndpoints.OpenApi;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky;
using Lagrange.Milky.Api;
using Lagrange.Milky.Caching;
using Lagrange.Milky.Configurations;
using Lagrange.Milky.Converters;
using Lagrange.Milky.Extensions;
using Lagrange.Milky.Events;
using Lagrange.Milky.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

internal static class McpSmoke
{
    public static async Task RunAsync()
    {
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(120));
        // FastEndpoints 的序列化配置每个进程只初始化一次，各种启动配置需要独立进程验证。
        foreach (string scenario in new[] { "disabled", "token", "empty" })
        {
            var start = new ProcessStartInfo(Environment.ProcessPath ?? throw new InvalidOperationException("Process path is unavailable."))
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            if (string.Equals(Path.GetFileNameWithoutExtension(start.FileName), "dotnet", StringComparison.OrdinalIgnoreCase))
                start.ArgumentList.Add(Environment.GetCommandLineArgs()[0]);
            start.ArgumentList.Add("--mcp-case");
            start.ArgumentList.Add(scenario);
            if (Environment.GetCommandLineArgs().Contains("--require-native"))
                start.ArgumentList.Add("--require-native");

            using var child = Process.Start(start) ?? throw new InvalidOperationException("MCP smoke process did not start.");
            var stdout = child.StandardOutput.ReadToEndAsync(deadline.Token);
            var stderr = child.StandardError.ReadToEndAsync(deadline.Token);
            try
            {
                await child.WaitForExitAsync(deadline.Token);
                string output = await stdout;
                string errors = await stderr;
                Console.Write(output);
                if (child.ExitCode != 0)
                    throw new InvalidOperationException($"MCP smoke case {scenario} exited with {child.ExitCode}: {errors}");
                Console.Error.Write(errors);
            }
            finally
            {
                if (!child.HasExited) child.Kill(entireProcessTree: true);
            }
        }
        Console.WriteLine("PASS: MCP HTTP discovery, authentication, schemas, endpoint invocation and errors");
    }

    public static async Task RunCaseAsync(string scenario)
    {
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        switch (scenario)
        {
            case "disabled":
                await CheckDisabledAsync(deadline.Token);
                break;
            case "token":
                await CheckEnabledAsync("mcp-smoke-token", deadline.Token);
                break;
            case "empty":
                await CheckEnabledAsync(null, deadline.Token);
                break;
            default:
                throw new ArgumentException($"Unknown MCP smoke case: {scenario}", nameof(scenario));
        }
    }

    private static async Task CheckDisabledAsync(CancellationToken ct)
    {
        await using var app = CreateHost(false, null);
        await app.StartAsync(ct);
        try
        {
            using var http = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };
            using var response = await InitializeAsync(http, ct);
            Check(response.StatusCode == HttpStatusCode.NotFound, "disabled MCP route returns 404");
        }
        finally { await app.StopAsync(CancellationToken.None); }
    }

    private static async Task CheckEnabledAsync(string? accessToken, CancellationToken ct)
    {
        await using var app = CreateHost(true, accessToken);
        await app.StartAsync(ct);
        try
        {
            using var http = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };
            if (accessToken is not null)
            {
                using var missing = await InitializeAsync(http, ct);
                Check(missing.StatusCode == HttpStatusCode.Unauthorized, "missing MCP token returns 401");
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "incorrect-token");
                using var incorrect = await InitializeAsync(http, ct);
                Check(incorrect.StatusCode == HttpStatusCode.Unauthorized, "incorrect MCP token returns 401");
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            var registered = ((IEndpointRouteBuilder)app).DataSources.SelectMany(source => source.Endpoints)
                .OfType<RouteEndpoint>().Where(endpoint => endpoint.RoutePattern.RawText?.StartsWith("/api/", StringComparison.Ordinal) == true).ToArray();
            Check(registered.Length > 0 && registered.All(endpoint => endpoint.Metadata.GetMetadata<FastEndpoints.Mcp.McpToolInfo>() is not null),
                "every API endpoint has MCP metadata");

            using var openApi = JsonDocument.Parse(await http.GetStringAsync("/openapi/v1.json", ct));
            var paths = openApi.RootElement.GetProperty("paths");
            Check(registered.All(endpoint => paths.TryGetProperty(endpoint.RoutePattern.RawText!, out _)), "OpenAPI covers every API route");
            Check(paths.GetProperty("/api/get_group_admin_permissions").TryGetProperty("post", out _) &&
                paths.GetProperty("/api/get_qzone_public_feed_detail").TryGetProperty("post", out _) &&
                paths.GetProperty("/api/reply_qzone_public_feed").TryGetProperty("post", out _), "OpenAPI includes typed group permissions and Qzone operations");

            await using var transport = new HttpClientTransport(new()
            {
                Endpoint = new Uri(http.BaseAddress, "/mcp"),
                TransportMode = HttpTransportMode.StreamableHttp
            }, http);
            await using var client = await McpClient.CreateAsync(transport, cancellationToken: ct);
            var tools = await client.ListToolsAsync(cancellationToken: ct);
            var expected = ((IEndpointRouteBuilder)app).DataSources.SelectMany(source => source.Endpoints)
                .OfType<RouteEndpoint>()
                .Select(endpoint => endpoint.RoutePattern.RawText)
                .Where(route => route is not null && route.StartsWith("/api/", StringComparison.Ordinal))
                .Select(route => route!)
                .Select(route => route[5..])
                .ToHashSet(StringComparer.Ordinal);
            var actual = tools.Select(tool => tool.Name).ToHashSet(StringComparer.Ordinal);
            Check(expected.Count > 0 && actual.SetEquals(expected) &&
                actual.SetEquals(Lagrange.Milky.Mcp.MilkyMcpTools.Names) && tools.Count == actual.Count,
                "MCP tools cover every API route exactly once",
                $"routes={expected.Count}, tools={tools.Count}, missing={string.Join(',', expected.Except(actual))}, extra={string.Join(',', actual.Except(expected))}");
            Check(tools.All(tool => tool.Name.Length is > 0 and <= 64 &&
                tool.Name.All(character => char.IsAsciiLetterOrDigit(character) || character is '_' or '-')),
                "every MCP tool has a valid name");
            Check(tools.All(tool => !string.IsNullOrWhiteSpace(tool.Description)), "every MCP tool has a description");
            Check(tools.All(tool => tool.ProtocolTool.InputSchema.GetProperty("type").GetString() == "object" &&
                !tool.ProtocolTool.InputSchema.GetProperty("additionalProperties").GetBoolean()),
                "every MCP tool exposes a strict object argument schema");

            var groupSchema = tools.Single(tool => tool.Name == "send_group_message").ProtocolTool.InputSchema;
            var groupProperties = groupSchema.GetProperty("properties");
            Check(groupProperties.TryGetProperty("group_id", out _) && groupProperties.TryGetProperty("message_style", out _),
                "MCP schema preserves snake_case parameter names");
            var required = groupSchema.GetProperty("required").EnumerateArray().Select(item => item.GetString()).ToArray();
            Check(required.Contains("group_id") && required.Contains("message"), "MCP schema exposes required message parameters");
            var segmentSchema = groupProperties.GetProperty("message").GetProperty("items");
            Check(Descendants(segmentSchema).Any(node => node.ValueKind == JsonValueKind.Object &&
                node.TryGetProperty("const", out var value) && value.ValueKind == JsonValueKind.String && value.GetString() == "text"),
                "MCP schema preserves polymorphic message discriminators");
            var packetProperties = tools.Single(tool => tool.Name == "send_packet").ProtocolTool.InputSchema.GetProperty("properties");
            Check(packetProperties.TryGetProperty("cmd", out _) && packetProperties.TryGetProperty("data", out _) &&
                packetProperties.TryGetProperty("rsp", out _), "MCP schema exposes raw packet parameters");

            var implementation = await CallAsync(client, "get_impl_info", "{}", ct);
            using var implementationJson = Body(implementation);
            Check(implementation.IsError == false && implementationJson.RootElement.GetProperty("retcode").GetInt64() == 0 &&
                implementationJson.RootElement.GetProperty("data").GetProperty("impl_name").GetString() == "Lagrange.Core",
                "MCP executes the real implementation-info endpoint");
            Check(tools.Single(tool => tool.Name == "get_impl_info").ProtocolTool.OutputSchema is not null &&
                implementation.StructuredContent is { } structured && structured.GetProperty("retcode").GetInt64() == 0,
                "MCP publishes output schema and structured content");

            var devices = await CallAsync(client, "get_online_devices", "{}", ct);
            using var devicesJson = Body(devices);
            Check(devices.IsError == false && !devicesJson.RootElement.GetProperty("data").GetProperty("observed").GetBoolean(),
                "MCP executes the real local device query");

            var businessFailure = await CallAsync(client, "send_packet", """{"cmd":""}""", ct);
            using var businessJson = Body(businessFailure);
            Check(businessFailure.IsError == false && businessJson.RootElement.GetProperty("status").GetString() == "failed" &&
                businessJson.RootElement.GetProperty("retcode").GetInt64() == -400,
                "MCP preserves HTTP-200 Milky business failures");
            var omittedResponseFlag = await CallAsync(client, "send_packet", """{"cmd":"Test.Mcp","data":"not-hex"}""", ct);
            using var omittedResponseJson = Body(omittedResponseFlag);
            Check(omittedResponseJson.RootElement.GetProperty("message").GetString()!.Contains("hexadecimal", StringComparison.Ordinal),
                "MCP preserves the omitted rsp default before payload validation");

            var unknownArgument = await CallAsync(client, "get_online_devices", """{"unexpected":true}""", ct);
            using var unknownJson = Body(unknownArgument);
            Check(unknownArgument.IsError == true && unknownJson.RootElement.GetProperty("validationErrors").GetArrayLength() > 0,
                "MCP rejects unknown arguments with validation errors");
            var missingRequired = await CallAsync(client, "send_group_message", "{}", ct);
            using var missingRequiredJson = Body(missingRequired);
            Check(missingRequired.IsError == true &&
                missingRequiredJson.RootElement.TryGetProperty("validationErrors", out var missingErrors) &&
                missingErrors.ValueKind == JsonValueKind.Array &&
                missingErrors.EnumerateArray().Any(error => error.GetProperty("property").GetString() == "message") &&
                missingErrors.EnumerateArray().Any(error => error.GetProperty("property").GetString() == "group_id"),
                "MCP rejects missing required message parameters with field validation errors before sending");
            var omittedArguments = await client.CallToolAsync(new CallToolRequestParams { Name = "send_group_message" }, ct);
            using var omittedArgumentsJson = Body(omittedArguments);
            Check(omittedArguments.IsError == true &&
                omittedArgumentsJson.RootElement.GetProperty("validationErrors").GetArrayLength() == 2,
                "MCP validates required fields when arguments are omitted");
            var partialArguments = await CallAsync(client, "send_group_message", """{"group_id":123}""", ct);
            using var partialArgumentsJson = Body(partialArguments);
            Check(partialArguments.IsError == true &&
                partialArgumentsJson.RootElement.GetProperty("validationErrors").GetArrayLength() > 0,
                "MCP preserves binding validation for incomplete JSON arguments");
            var exception = await CallAsync(client, "get_group_system_msg", """{"group_id":0}""", ct);
            using var exceptionJson = Body(exception);
            Check(exception.IsError == true && exceptionJson.RootElement.GetProperty("error").GetString() == "Endpoint invocation failed.",
                "MCP maps real endpoint exceptions without exposing exception details");
            Check(accessToken is null || http.DefaultRequestHeaders.Authorization?.Parameter == accessToken,
                accessToken is null ? "empty configured token permits MCP access" : "correct MCP bearer token permits access");
        }
        finally { await app.StopAsync(CancellationToken.None); }
    }

    private static WebApplication CreateHost(bool enabled, string? accessToken)
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Logging.AddSimpleConsole(options => options.SingleLine = true).SetMinimumLevel(LogLevel.Error);
        var settings = new MilkyConfiguration(new("127.0.0.1", 0), accessToken, api: new(mcp: new(enabled)));
        builder.Services.AddSingleton(settings);
        builder.Services.AddSingleton(_ => BotFactory.Create(new BotConfig { GetOptimumServer = false },
            BotKeystore.CreateEmpty(), BotAppInfo.ProtocolToAppInfo[Protocols.Linux]));
        builder.Services.AddSingleton<MessageCache>();
        builder.Services.AddSingleton<MilkyConverter>();
        builder.Services.AddSingleton<ResourceConverter>();
        builder.Services.AddSingleton<MilkyEventHub>();
        builder.Services.AddAuthentication("Milky")
            .AddScheme<AuthenticationSchemeOptions, MilkyAuthenticationHandler>("Milky", _ => { });
        builder.Services.AddAuthorization();
        builder.Services.AddFastEndpoints(DiscoveredTypes.All).OpenApiDocument(options =>
        {
            options.DocumentName = "v1";
            options.Title = "Lagrange.Milky API";
            options.Version = "v1";
            options.ExcludeNonFastEndpoints = true;
            options.ShortSchemaNames = true;
            options.EnableJWTBearerAuth = false;
        });
        builder.Services.AddMilkyMcp(settings);
        var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseFastEndpoints(options =>
        {
            options.ConfigureMilkyBinding();
            if (enabled) options.ConfigureMilkyMcp();
            options.Errors.ResponseBuilder = (failures, context, _) =>
            {
                context.Response.StatusCode = StatusCodes.Status200OK;
                return new MilkyApiResponse(-400, failures.FirstOrDefault()?.ErrorMessage ?? "request validation failed");
            };
        });
        app.UseMilkyMcp();
        app.MapOpenApi();
        return app;
    }

    private static Task<HttpResponseMessage> InitializeAsync(HttpClient http, CancellationToken ct)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/mcp")
        {
            Content = new StringContent("""{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2025-11-25","capabilities":{},"clientInfo":{"name":"Milky.McpSmoke","version":"1.0.0"}}}""", Encoding.UTF8, "application/json")
        };
        request.Headers.Accept.ParseAdd("application/json");
        request.Headers.Accept.ParseAdd("text/event-stream");
        return SendAsync();

        async Task<HttpResponseMessage> SendAsync()
        {
            using (request) return await http.SendAsync(request, ct);
        }
    }

    private static ValueTask<CallToolResult> CallAsync(McpClient client, string name, string json, CancellationToken ct)
    {
        using var document = JsonDocument.Parse(json);
        return client.CallToolAsync(new CallToolRequestParams
        {
            Name = name,
            Arguments = document.RootElement.EnumerateObject().ToDictionary(property => property.Name, property => property.Value.Clone())
        }, ct);
    }

    private static JsonDocument Body(CallToolResult result)
        => JsonDocument.Parse(result.Content.OfType<TextContentBlock>().Single().Text);

    private static IEnumerable<JsonElement> Descendants(JsonElement node)
    {
        yield return node;
        IEnumerable<JsonElement> children = node.ValueKind switch
        {
            JsonValueKind.Object => node.EnumerateObject().Select(property => property.Value),
            JsonValueKind.Array => node.EnumerateArray(),
            _ => []
        };
        foreach (var child in children)
            foreach (var descendant in Descendants(child))
                yield return descendant;
    }

    private static void Check(bool condition, string scenario, string? details = null)
    {
        if (!condition) throw new InvalidOperationException($"Milky MCP smoke failed: {scenario}{(details is null ? "" : ": " + details)}");
        Console.WriteLine($"PASS: {scenario}");
    }
}
