using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.Interaction;

[ApiHandler("set_input_status")]
public sealed class SetInputStatusHandler(BotContext lagrange) : INoResultApiHandler<SetInputStatusHandler.Request>
{
    private readonly BotContext _lagrange = lagrange;

    public async ValueTask<MilkyApiResponse> HandleAsync(Request request, CancellationToken ct)
    {
        await _lagrange.SetInputStatus(request.UserId, checked((uint)request.EventType), ct);
        return new MilkyApiResponse();
    }

    public sealed class Request(long userId, int eventType)
    {
        [JsonPropertyName("user_id")] public required long UserId { get; init; } = userId;
        [JsonPropertyName("event_type")] public required int EventType { get; init; } = eventType;
    }
}
