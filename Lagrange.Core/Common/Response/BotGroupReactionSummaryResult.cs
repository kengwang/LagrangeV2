namespace Lagrange.Core.Common.Response;

public sealed class BotGroupReactionSummaryResult
{
    public required IReadOnlyList<BotGroupReactionSummaryEntry> Entries { get; init; }
}

public sealed class BotGroupReactionSummaryEntry
{
    public required string EmojiId { get; init; }
    public uint EmojiType { get; init; }
    public uint Count { get; init; }
    public ulong LastReactionTime { get; init; }
}
