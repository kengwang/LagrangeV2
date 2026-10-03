namespace Lagrange.Core.Common.Response;

public sealed class BotProfileLikeResult
{
    public required string Uid { get; init; }
    public long Time { get; init; }
    public int TotalCount { get; init; }
    public int NewCount { get; init; }
    public IReadOnlyList<BotProfileLikeUser> Users { get; init; } = [];
}

public sealed class BotProfileLikeUser
{
    public required string Uid { get; init; }
    public long Uin { get; init; }
    public string Nickname { get; init; } = string.Empty;
    public long LatestTime { get; init; }
    public int Count { get; init; }
    public bool IsFriend { get; init; }
}
