namespace Lagrange.Core.Common.Response;

public sealed class BotGroupAnnouncementResult
{
    public required IReadOnlyList<BotGroupAnnouncement> Announcements { get; init; }
}

public sealed class BotGroupAnnouncement
{
    public string Id { get; init; } = string.Empty;
    public long PublisherId { get; init; }
    public long PublishTime { get; init; }
    public string Text { get; init; } = string.Empty;
    public bool Pinned { get; init; }
    public int ReadCount { get; init; }
}
