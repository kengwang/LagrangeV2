namespace Lagrange.Core.Common.Response;

public sealed class BotEmojiLikesResult
{
    public required IReadOnlyList<BotEmojiLikeUser> Users { get; init; }
    public string Cookie { get; init; } = string.Empty;
    public bool IsLast { get; init; }
}

public sealed class BotEmojiLikeUser
{
    public required long Uin { get; init; }
    public required string Nickname { get; init; }
    public required string HeadUrl { get; init; }
}
