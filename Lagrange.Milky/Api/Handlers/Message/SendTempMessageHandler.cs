using FastEndpoints;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Message;
using Lagrange.Milky.Converters;
using Lagrange.Milky.Extensions;
using Lagrange.Milky.Models;
using Lagrange.Milky.Models.Segments;

namespace Lagrange.Milky.Api.Handlers.Message;

public sealed class SendTempMessageHandler(BotContext lagrange, MilkyConverter converter) : Endpoint<SendTempMessageHandler.Request, MilkyApiResponse<SendTempMessageHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/send_temp_message");
    }
    private readonly BotContext _lagrange = lagrange;
    private readonly MilkyConverter _converter = converter;

    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        var chain = await _converter.FromOutgoingSegmentsAsync(request.Message, MessageType.Temp, request.UserId, ct);
        var message = await _lagrange.SendTempMessage(request.GroupId, request.UserId, chain, ct);
        return new MilkyApiResponse<Result>(new Result
        {
            MessageSeq = (long)message.ClientSequence,
            Time = message.Time,
        });
    }

    public sealed class Request(long groupId, long userId, IReadOnlyList<OutgoingSegmentBase> message)
    {
        [JsonPropertyName("group_id")] public required long GroupId { get; init; } = groupId;
        [JsonPropertyName("user_id")] public required long UserId { get; init; } = userId;
        [JsonPropertyName("message")] public required IReadOnlyList<OutgoingSegmentBase> Message { get; init; } = message;
    }

    public sealed class Result
    {
        [JsonPropertyName("message_seq")] public required long MessageSeq { get; init; }
        [JsonPropertyName("time")] public required long Time { get; init; }
    }
}
