namespace Lagrange.Core.Common.Response;

public sealed class BotQzoneFeedResult
{
    public required IReadOnlyList<BotQzoneFeed> Feeds { get; init; }
    public bool HasMore { get; init; }
    public string? NextCursor { get; init; }
}
public sealed class BotQzoneFeed
{
    public long UserId { get; init; }
    public string Nickname { get; init; } = string.Empty;
    public long Time { get; init; }
    public int AppId { get; init; }
    public string Key { get; init; } = string.Empty;
    public string Html { get; init; } = string.Empty;
    public string? MessageId { get; internal set; }
    public string? Content { get; internal set; }
    public bool? ContentComplete { get; internal set; }
    public IReadOnlyList<string>? Images { get; internal set; }
    public long? LikeCount { get; internal set; }
    public long? CommentCount { get; internal set; }
}
