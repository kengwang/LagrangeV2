using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using FastEndpoints;
using FluentValidation.Results;
using Lagrange.Milky;
using Lagrange.Milky.Api.Handlers.Group;
using Lagrange.Milky.Api.Handlers.Message;
using Lagrange.Milky.Api.Handlers.System;
using Lagrange.Milky.Extensions;
using Lagrange.Milky.Models.Segments;
using Lagrange_Milky;
using Microsoft.AspNetCore.Http.Features;

// This executable does not start a listener, create a bot, or execute any QQ endpoint.
// Every generic binder is statically instantiated so Native AOT compiles the real binding path.
if (args.Contains("--require-native") && RuntimeFeature.IsDynamicCodeSupported)
    throw new InvalidOperationException("Run the published Native AOT executable for --require-native.");

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

    Console.WriteLine($"PASS: 12 request-binding smoke scenarios; DynamicCodeSupported={RuntimeFeature.IsDynamicCodeSupported}");
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
    if (!condition) throw new InvalidOperationException($"Binding smoke failed: {scenario}");
    Console.WriteLine($"PASS: {scenario}");
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
