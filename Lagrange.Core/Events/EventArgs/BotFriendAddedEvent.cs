namespace Lagrange.Core.Events.EventArgs;

public sealed class BotFriendAddedEvent(long userUin, string userUid, string nickname, long time) : EventBase
{
    public long UserUin { get; } = userUin;
    public string UserUid { get; } = userUid;
    public string Nickname { get; } = nickname;
    public long Time { get; } = time;
    public override string ToEventMessage() => $"{nameof(BotFriendAddedEvent)}: UserUin={UserUin}, UserUid={UserUid}";
}
