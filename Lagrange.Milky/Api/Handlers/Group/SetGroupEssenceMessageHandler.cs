using FastEndpoints;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Message;
using Lagrange.Milky.Caching;

namespace Lagrange.Milky.Api.Handlers.Group;

public sealed class SetGroupEssenceMessageHandler(BotContext lagrange, MessageCache cache) : Endpoint<SetGroupEssenceMessageHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/set_group_essence_message");
    }
    private readonly BotContext _lagrange = lagrange;
    private readonly MessageCache _cache = cache;

    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
    {
        var message = _cache.Get(MessageType.Group, request.GroupId, (ulong)request.MessageSeq)
            ?? (await _lagrange.GetGroupMessage(request.GroupId, (ulong)request.MessageSeq, (ulong)request.MessageSeq)
                .WaitAsync(ct))
                .FirstOrDefault();
        if (message == null) return new MilkyApiResponse(-404, "message not found");
        await _lagrange.SetEssenceMessage(message, ct);
        return new MilkyApiResponse();
    }

    public sealed class Request(long groupId, long messageSeq, bool isSet = true)
    {
        [JsonPropertyName("group_id")] public required long GroupId { get; init; } = groupId;
        [JsonPropertyName("message_seq")] public required long MessageSeq { get; init; } = messageSeq;
        [JsonPropertyName("is_set")] public bool IsSet { get; init; } = isSet;
    }
}
