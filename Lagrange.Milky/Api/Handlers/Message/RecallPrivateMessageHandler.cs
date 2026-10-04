using FastEndpoints;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Message;
using Lagrange.Milky.Caching;

namespace Lagrange.Milky.Api.Handlers.Message;

public sealed class RecallPrivateMessageHandler(BotContext lagrange, MessageCache cache) : Endpoint<RecallPrivateMessageHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/recall_private_message");
    }
    private readonly BotContext _lagrange = lagrange;
    private readonly MessageCache _cache = cache;

    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
    {
        var message = _cache.Get(MessageType.Private, request.UserId, (ulong)request.MessageSeq)
            ?? (await _lagrange.GetC2CMessage(
                    request.UserId,
                    (ulong)request.MessageSeq,
                    (ulong)request.MessageSeq
                ).WaitAsync(ct))
                .FirstOrDefault();
        if (message == null) return new MilkyApiResponse(-404, "Message not found");
        await _lagrange.RecallMessage(message, ct);
        return new MilkyApiResponse();
    }

    public sealed class Request(long userId, long messageSeq)
    {
        [JsonPropertyName("user_id")] public required long UserId { get; init; } = userId;
        [JsonPropertyName("message_seq")] public required long MessageSeq { get; init; } = messageSeq;
    }
}
