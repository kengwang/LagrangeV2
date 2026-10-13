using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using FastEndpoints;
using FluentValidation.Results;
using Lagrange.Milky;
using Lagrange.Milky.Api.Handlers.Group;
using Lagrange.Milky.Api.Handlers.Message;
using Lagrange.Milky.Api.Handlers.System;
using Lagrange.Milky.Extensions;
using Lagrange.Milky.Models.Segments;
using Lagrange.Milky.Models;
using Lagrange.Milky.Events;
using Lagrange.Milky.Serialization;
using Lagrange_Milky;
using Microsoft.AspNetCore.Http.Features;

// This executable does not start a listener, create a bot, or execute any QQ endpoint.
// Every generic binder is statically instantiated so Native AOT compiles the real binding path.
if (args.Contains("--require-native") && RuntimeFeature.IsDynamicCodeSupported)
    throw new InvalidOperationException("Run the published Native AOT executable for --require-native.");

await CheckLoggingAsync();

var builder = WebApplication.CreateSlimBuilder();
builder.Logging.ClearProviders();
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddFastEndpoints([SmokeEndpointMetadata.Type]);
await using var app = builder.Build();
app.UseFastEndpoints(options =>
{
#if REPRODUCE_UNCACHED_BINDING
    // Negative Native AOT regression variant. Intentionally excludes binding metadata and parsers.
    options.Serializer.Options.AddSerializerContextsFromLagrange_Milky();
#else
    options.ConfigureMilkyBinding();
#endif
});

using var scope = app.Services.CreateScope();
var bodies = new List<Stream>();
try
{
    var groupContext = Context("""{"group_id":123,"message":[{"type":"text","data":{"text":"group smoke"}}]}""");
    var group = await new RequestBinder<SendGroupMessageHandler.Request>().BindAsync(groupContext, CancellationToken.None);
    Check(group.GroupId == 123 && group.Message.Single() is TextOutgoingSegment { Data.Text: "group smoke" }, "send_group_message JSON and polymorphic collection");

    var privateContext = Context("""{"user_id":456,"message":[{"type":"text","data":{"text":"private smoke"}}]}""");
    var friend = await new RequestBinder<SendPrivateMessageHandler.Request>().BindAsync(privateContext, CancellationToken.None);
    Check(friend.UserId == 456 && friend.Message.Single() is TextOutgoingSegment { Data.Text: "private smoke" }, "send_private_message JSON");

    var historyContext = Context("""{"message_scene":"group","peer_id":123,"start_message_seq":77,"limit":5}""");
    var history = await new RequestBinder<GetHistoryMessagesHandler.Request>().BindAsync(historyContext, CancellationToken.None);
    Check(history.MessageScene == "group" && history.StartMessageSeq == 77 && history.Limit == 5, "string and nullable numeric JSON");

    var nullContext = Context("""{"message_scene":"friend","peer_id":456,"start_message_seq":null,"limit":3}""");
    var nullHistory = await new RequestBinder<GetHistoryMessagesHandler.Request>().BindAsync(nullContext, CancellationToken.None);
    Check(nullHistory.StartMessageSeq is null, "explicit nullable numeric null");

    var muteContext = Context("""{"group_id":123,"is_mute":false}""");
    var mute = await new RequestBinder<SetGroupWholeMuteHandler.Request>().BindAsync(muteContext, CancellationToken.None);
    Check(mute.GroupId == 123 && !mute.IsMute, "false boolean JSON");

    var cookieContext = Context("""{"domains":["qq.com","qzone.qq.com"]}""");
    var cookies = await new RequestBinder<GetCookiesHandler.Request>().BindAsync(cookieContext, CancellationToken.None);
    Check(cookies.Domains.SequenceEqual(["qq.com", "qzone.qq.com"]), "string collection JSON");

    // Exercise the non-JSON parsers too: these used to initialize reflection-based IParsable paths.
    var queryContext = Context("""{"message_scene":"group","peer_id":123}""", "?MessageScene=friend&PeerId=456&StartMessageSeq=88&Limit=4");
    var query = await new RequestBinder<GetHistoryMessagesHandler.Request>().BindAsync(queryContext, CancellationToken.None);
    Check(query.MessageScene == "friend" && query.PeerId == 456 && query.StartMessageSeq == 88 && query.Limit == 4, "query string and nullable numeric parsers");

    var boolContext = Context("""{"group_id":1}""", "?GroupId=123&IsMute=false");
    var queryMute = await new RequestBinder<SetGroupWholeMuteHandler.Request>().BindAsync(boolContext, CancellationToken.None);
    Check(queryMute.GroupId == 123 && !queryMute.IsMute, "query bool parser");

    var uintContext = Context("""{"group_id":123}""", "?ChatType=4294967295");
    var voice = await new RequestBinder<GetAiVoiceListHandler.Request>().BindAsync(uintContext, CancellationToken.None);
    Check(voice.ChatType == uint.MaxValue, "nullable uint query parser");

    var ulongContext = Context("""{"group_id":123}""", "?MessageSeq=18446744073709551615");
    var todo = await new RequestBinder<CompleteGroupTodoHandler.Request>().BindAsync(ulongContext, CancellationToken.None);
    Check(todo.MessageSeq == ulong.MaxValue, "nullable ulong query parser");

    var dateContext = Context("""{"group_id":123}""", "?Day=2026-10-05");
    var signIn = await new RequestBinder<GetGroupSignInHandler.Request>().BindAsync(dateContext, CancellationToken.None);
    Check(signIn.Day == new DateTime(2026, 10, 5), "nullable DateTime query parser");

    var invalidContext = Context("""{"message_scene":"group","peer_id":123}""", "?Limit=not-a-number");
    bool rejected = false;
    try
    {
        await new RequestBinder<GetHistoryMessagesHandler.Request>().BindAsync(invalidContext, CancellationToken.None);
        rejected = invalidContext.ValidationFailures.Count > 0;
    }
    catch (ValidationFailureException) { rejected = true; }
    Check(rejected, "invalid numeric query rejected");

    var packetContext = Context("""{"cmd":"Example.Command","data":"0801aB00","rsp":false}""");
    var packet = await new RequestBinder<SendPacketHandler.Request>().BindAsync(packetContext, CancellationToken.None);
    Check(packet.Command == "Example.Command" && packet.Data == "0801aB00" && packet.WaitForResponse == false, "send_packet JSON and nullable boolean");
    var packetDefaults = await new RequestBinder<SendPacketHandler.Request>().BindAsync(Context("""{"cmd":"Example.Command"}"""), CancellationToken.None);
    Check(packetDefaults.Data == string.Empty && packetDefaults.WaitForResponse == true, "send_packet omitted defaults");
    var nullPacket = await new RequestBinder<SendPacketHandler.Request>().BindAsync(Context("""{"cmd":"Example.Command","data":null,"rsp":null}"""), CancellationToken.None);
    Check(nullPacket.Data is null && nullPacket.WaitForResponse is null, "send_packet explicit nulls preserved for validation");
    var rawData = new RawPacketData { Command = "Example.Command", Data = "0801AB00", Sequence = 1, RetCode = 7, Extra = "error" };
    var packetEvent = Serializer.JsonSerialize(new MilkyEvent { EventType = "packet_received", Time = 1, SelfId = 123, Data = rawData });
    Check(packetEvent.Contains("\"ret_code\":7") && packetEvent.Contains("\"data\":\"0801AB00\""), "raw packet event generated serialization");
    var packetResponse = Serializer.JsonSerialize(new Lagrange.Milky.Api.MilkyApiResponse<RawPacketData>(rawData));
    Check(packetResponse.Contains("\"ret_code\":7"), "raw packet response generated serialization");

    var encryptKey = await new RequestBinder<RequestDecryptKeyHandler.Request>().BindAsync(Context("""{"mode":"encrypt"}"""), CancellationToken.None);
    Check(encryptKey.Mode == "encrypt" && encryptKey.KeyMeta is null, "request_decrypt_key encrypt binding");
    string keyMeta = new('a', 128);
    var decryptKey = await new RequestBinder<RequestDecryptKeyHandler.Request>().BindAsync(Context("{\"mode\":\"decrypt\",\"key_meta\":\"" + keyMeta + "\"}"), CancellationToken.None);
    Check(decryptKey.Mode == "decrypt" && decryptKey.KeyMeta == keyMeta, "request_decrypt_key decrypt binding");
    var keyResponse = Serializer.JsonSerialize(new Lagrange.Milky.Api.MilkyApiResponse<RequestDecryptKeyHandler.Result>(
        new RequestDecryptKeyHandler.Result { Key = "synthetic-key", KeyMeta = keyMeta }));
    Check(keyResponse.Contains("\"key\":\"synthetic-key\"") && keyResponse.Contains("\"key_meta\":\"" + keyMeta + "\""), "request_decrypt_key generated response serialization");

    var friendsStatusResponse = Serializer.JsonSerialize(new Lagrange.Milky.Api.MilkyApiResponse<GetFriendsStatusHandler.Result>(new()
    {
        SelfUid = "u_smoke_self",
        Friends = [new() { Uid = "u_smoke_friend", Status = null, StatusText = "电脑在线", RawFields =
            [new() { Field = 7, WireType = 0, Value = "18446744073709551615" }, new() { Field = 21, WireType = 2, Value = "0800" }] }]
    }));
    using var friendsStatusJson = JsonDocument.Parse(friendsStatusResponse);
    var statusItem = friendsStatusJson.RootElement.GetProperty("data").GetProperty("friends")[0];
    Check(statusItem.GetProperty("status").ValueKind == JsonValueKind.Null &&
        statusItem.GetProperty("status_text").GetString() == "电脑在线" &&
        statusItem.GetProperty("raw_fields")[0].GetProperty("value").GetString() == "18446744073709551615" &&
        statusItem.GetProperty("raw_fields")[1].GetProperty("wire_type").GetByte() == 2,
        "get_friends_status generated response, nullable status and raw fields");

    Console.WriteLine($"PASS: 21 binding and serialization smoke scenarios; DynamicCodeSupported={RuntimeFeature.IsDynamicCodeSupported}");
}
finally
{
    foreach (var body in bodies) body.Dispose();
}

BinderContext Context(string json, string? query = null)
{
    var body = new MemoryStream(Encoding.UTF8.GetBytes(json));
    bodies.Add(body);
    var http = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
    http.Features.Set<IHttpRequestBodyDetectionFeature>(new BodyDetection());
    http.Request.Method = "POST";
    http.Request.ContentType = "application/json";
    http.Request.ContentLength = body.Length;
    http.Request.Body = body;
    if (query is not null) http.Request.QueryString = new QueryString(query);
    return new BinderContext(http, new List<ValidationFailure>(), null, false, []);
}

static void Check(bool condition, string scenario)
{
    if (!condition) throw new InvalidOperationException($"Milky smoke failed: {scenario}");
    Console.WriteLine($"PASS: {scenario}");
}

static async Task CheckLoggingAsync()
{
    string directory = Directory.CreateTempSubdirectory("milky-aot-logging-").FullName;
    try
    {
        var loggingBuilder = WebApplication.CreateSlimBuilder(new WebApplicationOptions { ContentRootPath = directory, Args = [] });
        loggingBuilder.Configuration.Sources.Clear();
        loggingBuilder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Logging:LogLevel:Default"] = "Information",
            ["Logging:JsonFile:Enabled"] = "true",
        });
        using (var rootLogger = loggingBuilder.ConfigureMilkyLogging())
        {
            await using var loggingApp = loggingBuilder.Build();
            var logger = loggingApp.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Milky.AotSmoke");
            logger.LogDebug("filtered-marker");
            using (logger.BeginScope(new Dictionary<string, object> { ["RequestId"] = "native-smoke" }))
                logger.LogError(new InvalidOperationException("exception-marker"), "JSON smoke {Value}", 42);
        }
        var lines = Directory.GetFiles(directory, "*.jsonl", SearchOption.AllDirectories).SelectMany(File.ReadAllLines).ToArray();
        Check(lines.Length == 1, "JSON logging category filter and shutdown flush");
        using var document = JsonDocument.Parse(lines[0]);
        var entry = document.RootElement;
        var properties = entry.GetProperty("Properties");
        Check(entry.GetProperty("MessageTemplate").GetString() == "JSON smoke {Value}" &&
            entry.GetProperty("RenderedMessage").GetString() == "JSON smoke 42" &&
            entry.GetProperty("Exception").GetString()!.Contains("exception-marker") &&
            properties.GetProperty("Value").GetInt32() == 42 &&
            properties.GetProperty("SourceContext").GetString() == "Milky.AotSmoke" &&
            properties.GetProperty("RequestId").GetString() == "native-smoke", "JSON logging structured properties, scope and exception");
    }
    finally
    {
        Directory.Delete(directory, recursive: true);
    }
}

sealed class BodyDetection : IHttpRequestBodyDetectionFeature
{
    public bool CanHaveBody => true;
}

public sealed class BindingInitializationEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/__binding_smoke_initialization");
        AllowAnonymous();
    }

    public override Task HandleAsync(CancellationToken ct) => Task.CompletedTask;
}

static class SmokeEndpointMetadata
{
    // Equivalent to the endpoint generator's preservation helper, including inherited members.
    public static Type Type => Preserve<BindingInitializationEndpoint>();

    private static Type Preserve<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors)] T>()
        => typeof(T);
}
