namespace Lagrange.Core.Events.EventArgs;

public sealed class BotGroupSpecialTitleChangeEvent(long groupUin, long memberUin, long operatorUin, string title) : EventBase
{
    public long GroupUin { get; } = groupUin;
    public long MemberUin { get; } = memberUin;
    public long OperatorUin { get; } = operatorUin;
    public string Title { get; } = title;

    public override string ToEventMessage() => $"{nameof(BotGroupSpecialTitleChangeEvent)}: GroupUin={GroupUin}, MemberUin={MemberUin}, Title={Title}";
}
