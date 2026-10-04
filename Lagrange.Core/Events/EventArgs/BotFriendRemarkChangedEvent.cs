namespace Lagrange.Core.Events.EventArgs;

public sealed class BotFriendRemarkChangedEvent(long userUin, string userUid, string remark) : EventBase
{
    public long UserUin { get; } = userUin;
    public string UserUid { get; } = userUid;
    public string Remark { get; } = remark;

    public override string ToEventMessage() => $"{nameof(BotFriendRemarkChangedEvent)}: UserUin={UserUin}, Remark={Remark}";
}
