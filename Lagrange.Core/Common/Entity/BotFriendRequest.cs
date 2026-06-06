namespace Lagrange.Core.Common.Entity;

public class BotFriendRequest(string targetUid, string sourceUid, uint eventState, string comment, string source, uint time)
{
    public string TargetUid { get; } = targetUid;

    public long TargetUin { get; init; }

    public string SourceUid { get; } = sourceUid;

    public long SourceUin { get; init; }

    public State EventState { get; } = (State)eventState;

    public string Comment { get; } = comment;

    public string Source { get; } = source;

    public DateTime Time { get; } = DateTimeOffset.FromUnixTimeSeconds(time).UtcDateTime;

    public enum State
    {
        Pending = 1,
        Disapproved = 2,
        Approved = 3,
    }
}
