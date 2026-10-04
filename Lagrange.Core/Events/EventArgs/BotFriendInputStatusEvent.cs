namespace Lagrange.Core.Events.EventArgs;

public sealed class BotFriendInputStatusEvent(long userUin, string userUid, uint eventType) : EventBase
{
    public long UserUin { get; } = userUin;
    public string UserUid { get; } = userUid;
    public uint EventType { get; } = eventType;
    public string StatusText => EventType == 3 ? "对方正在讲话..." : "对方正在输入...";

    public override string ToEventMessage() => $"{nameof(BotFriendInputStatusEvent)}: UserUin={UserUin}, EventType={EventType}";
}
