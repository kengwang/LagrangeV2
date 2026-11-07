using FastEndpoints;
using FastEndpoints.Mcp;
using Lagrange.Milky.Configurations;
using Lagrange.Milky.Mcp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using System.Text.Json.Nodes;

namespace Lagrange.Milky.Extensions;

public static class MilkyMcpExtensions
{
    public static IServiceCollection AddMilkyMcp(this IServiceCollection services, MilkyConfiguration configuration)
    {
        if (configuration.Api.Mcp.Enabled)
        {
            services.AddMcp();
            services.AddMcpServer().WithRequestFilters(filters => filters.AddCallToolFilter(next => async (context, ct) =>
            {
                ct.ThrowIfCancellationRequested();
                // 空参数不会触发桥接器的 JSON 反序列化，需要检查生成元数据中的必填字段。
                var required = MilkyMcpTools.GetRequiredArguments(context.Params.Name);
                if (context.Params.Arguments is { Count: > 0 } || required.Length == 0)
                {
                    if (!configuration.Api.Mcp.IncludeExceptionDetails)
                        return await next(context, ct);

                    var previous = MilkyMcpExceptionCapture.Current.Value;
                    var capture = new MilkyMcpExceptionCapture();
                    MilkyMcpExceptionCapture.Current.Value = capture;
                    try
                    {
                        return capture.WithDetails(await next(context, ct), context.Params.Name);
                    }
                    finally
                    {
                        MilkyMcpExceptionCapture.Current.Value = previous;
                    }
                }

                var errors = new JsonObject
                {
                    ["validationErrors"] = new JsonArray(required.Select(name => (JsonNode)new JsonObject
                    {
                        ["property"] = name,
                        ["error"] = $"A value for '{name}' is required.",
                        ["code"] = "Required"
                    }).ToArray())
                };
                return new CallToolResult
                {
                    IsError = true,
                    Content = [new TextContentBlock { Text = errors.ToJsonString() }]
                };
            }));
        }
        return services;
    }

    public static void ConfigureMilkyMcp(this Config configuration)
    {
        configuration.Serializer.Options.TypeInfoResolverChain.Insert(0,
            new MilkyMcpJsonContext(new(configuration.Serializer.Options)));
        // 仅复用 SDK 生成的协议元数据，保留 API DTO 的生成式序列化绑定。
        configuration.Serializer.Options.TypeInfoResolverChain.Insert(0,
            McpJsonUtilities.DefaultOptions.TypeInfoResolverChain[0]);
        configuration.Endpoints.Configurator = definition =>
        {
            MilkyMcpTools.Configure(definition, configuration.Serializer.Options);
            definition.PostProcessor<MilkyMcpExceptionCapture>(Order.After);
        };
    }

    public static void UseMilkyMcp(this WebApplication app)
    {
        if (app.Services.GetRequiredService<MilkyConfiguration>().Api.Mcp.Enabled)
            app.UseMcp(configureRoute: route => route.RequireAuthorization(
                new AuthorizationPolicyBuilder("Milky").RequireAuthenticatedUser().Build()));
    }
}
