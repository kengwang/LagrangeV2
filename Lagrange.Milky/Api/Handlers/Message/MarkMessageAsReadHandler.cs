using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.Message;

[ApiHandler("mark_message_as_read")]
public sealed class MarkMessageAsReadHandler(BotContext lagrange) : INoResultApiHandler<MarkMessageAsReadHandler.Request>
{
    public async ValueTask<MilkyApiResponse> HandleAsync(Request request, CancellationToken ct)
    {
        await lagrange.MarkMessageAsRead(request.MessageScene, request.PeerId, request.LastReadSeq, ct).WaitAsync(ct);
        return new MilkyApiResponse();
    }

    public sealed class Request(string messageScene, long peerId, ulong lastReadSeq)
    {
        [JsonPropertyName("message_scene")] public required string MessageScene { get; init; } = messageScene;
        [JsonPropertyName("peer_id")] public long PeerId { get; init; } = peerId;
        [JsonPropertyName("last_read_seq")] public ulong LastReadSeq { get; init; } = lastReadSeq;
    }
}
