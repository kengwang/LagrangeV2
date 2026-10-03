namespace Lagrange.Core.Common.Response;

public sealed class BotQzoneFeedResult
{
    public required IReadOnlyList<BotQzoneFeed> Feeds { get; init; }
    public bool HasMore { get; init; }
}
public sealed class BotQzoneFeed
{
    public long UserId { get; init; }
    public string Nickname { get; init; } = string.Empty;
    public long Time { get; init; }
    public int AppId { get; init; }
    public string Key { get; init; } = string.Empty;
    public string Html { get; init; } = string.Empty;
}
