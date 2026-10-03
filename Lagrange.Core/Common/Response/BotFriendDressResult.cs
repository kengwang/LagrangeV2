namespace Lagrange.Core.Common.Response;

public sealed class BotFriendDressResult
{
    public required string TargetUin { get; init; }
    public bool IsSvip { get; init; }
    public string AvatarUrl { get; init; } = string.Empty;
    public required IReadOnlyList<BotFriendDressItem> Items { get; init; }
}

public sealed class BotFriendDressItem
{
    public int AppId { get; init; }
    public string Kind { get; init; } = string.Empty;
    public int ItemId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string PreviewUrl { get; init; } = string.Empty;
    public string VideoUrl { get; init; } = string.Empty;
    public int Price { get; init; }
}
