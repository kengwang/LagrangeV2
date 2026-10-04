using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.Message;

public sealed class MarkMessageAsReadHandler(BotContext lagrange) : Endpoint<MarkMessageAsReadHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/mark_message_as_read");
    }
    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
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
