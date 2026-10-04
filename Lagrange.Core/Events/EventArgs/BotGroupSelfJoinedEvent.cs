namespace Lagrange.Core.Events.EventArgs;

public sealed class BotGroupSelfJoinedEvent(long groupUin, long operatorUin, string operatorUid) : EventBase
{
    public long GroupUin { get; } = groupUin;
    public long OperatorUin { get; } = operatorUin;
    public string OperatorUid { get; } = operatorUid;
    public override string ToEventMessage() => $"{nameof(BotGroupSelfJoinedEvent)}: GroupUin={GroupUin}, OperatorUin={OperatorUin}";
}
