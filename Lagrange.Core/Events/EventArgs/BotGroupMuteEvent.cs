namespace Lagrange.Core.Events.EventArgs;

public class BotGroupMuteEvent(long groupUin, long operatorUin, long userUin, uint duration) : EventBase
{
    public long GroupUin { get; } = groupUin;
    public long OperatorUin { get; } = operatorUin;
    public long UserUin { get; } = userUin;
    public uint Duration { get; } = duration;
    public bool IsWholeGroup => UserUin == 0;
    public override string ToEventMessage() => $"{nameof(BotGroupMuteEvent)}: {GroupUin}/{UserUin} for {Duration}s";
}

public sealed class BotGroupWholeMuteEvent(long groupUin, long operatorUin, uint duration)
    : BotGroupMuteEvent(groupUin, operatorUin, 0, duration);
