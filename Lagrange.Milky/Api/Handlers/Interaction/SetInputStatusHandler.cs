using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.Interaction;

public sealed class SetInputStatusHandler(BotContext lagrange) : Endpoint<SetInputStatusHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/set_input_status");
    }
    private readonly BotContext _lagrange = lagrange;

    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
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
