using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.Interaction;

[ApiHandler("get_group_reaction_summary")]
public sealed class GetGroupReactionSummaryHandler(BotContext lagrange) : IApiHandler<GetGroupReactionSummaryHandler.Request, GetGroupReactionSummaryHandler.Result>
{
    private readonly BotContext _lagrange = lagrange;

    public async ValueTask<MilkyApiResponse<Result>> HandleAsync(Request request, CancellationToken ct)
    {
        var result = await _lagrange.GetGroupReactionSummary(request.GroupId, checked((ulong)request.MessageSeq), ct).WaitAsync(ct);
        return new MilkyApiResponse<Result>(new Result
        {
            Entries = [.. result.Entries.Select(entry => new Entry
            {
                EmojiId = entry.EmojiId,
                EmojiType = entry.EmojiType,
                Count = entry.Count,
                LastReactionTime = entry.LastReactionTime,
            })],
        });
    }

    public sealed class Request(long groupId, long messageSeq)
    {
        [JsonPropertyName("group_id")] public required long GroupId { get; init; } = groupId;
        [JsonPropertyName("message_seq")] public required long MessageSeq { get; init; } = messageSeq;
    }

    public sealed class Result
    {
        [JsonPropertyName("entries")] public required IReadOnlyList<Entry> Entries { get; init; }
    }

    public sealed class Entry
    {
        [JsonPropertyName("emoji_id")] public required string EmojiId { get; init; }
        [JsonPropertyName("emoji_type")] public uint EmojiType { get; init; }
        [JsonPropertyName("count")] public uint Count { get; init; }
        [JsonPropertyName("last_reaction_time")] public ulong LastReactionTime { get; init; }
    }
}
