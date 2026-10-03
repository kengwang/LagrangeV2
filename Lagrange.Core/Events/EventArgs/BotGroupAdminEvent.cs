namespace Lagrange.Core.Events.EventArgs;

public sealed class BotGroupAdminEvent(long groupUin, long userUin, bool isPromote) : EventBase
{
    public long GroupUin { get; } = groupUin;
    public long UserUin { get; } = userUin;
    public bool IsPromote { get; } = isPromote;
    public override string ToEventMessage() => $"{nameof(BotGroupAdminEvent)}: {GroupUin}/{UserUin} {(IsPromote ? "promoted" : "demoted")}";
}
