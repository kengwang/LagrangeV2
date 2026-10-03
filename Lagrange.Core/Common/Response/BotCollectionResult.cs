namespace Lagrange.Core.Common.Response;

public sealed class BotCollectionResult
{
    public required IReadOnlyList<BotCollectionItem> Items { get; init; }
    public uint TotalCount { get; init; }
    public bool ReachedBottom { get; init; }
}

public sealed class BotCollectionItem
{
    public required string Id { get; init; }
    public uint Type { get; init; }
    public string? Text { get; init; }
    public string? ShareUrl { get; init; }
    public ulong CreateTime { get; init; }
    public ulong CollectTime { get; init; }
    public ulong ModifyTime { get; init; }
    public string? AuthorUid { get; init; }
    public long AuthorUin { get; init; }
}
