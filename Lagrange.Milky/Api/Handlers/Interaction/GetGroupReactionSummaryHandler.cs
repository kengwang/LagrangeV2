using FastEndpoints;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.Interaction;

public sealed class GetGroupReactionSummaryHandler(BotContext lagrange) : Endpoint<GetGroupReactionSummaryHandler.Request, MilkyApiResponse<GetGroupReactionSummaryHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/get_group_reaction_summary");
    }
    private readonly BotContext _lagrange = lagrange;

    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
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
