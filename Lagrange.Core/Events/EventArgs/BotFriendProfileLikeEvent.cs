namespace Lagrange.Core.Events.EventArgs;

public sealed class BotFriendProfileLikeEvent(long operatorUin, string nickname, int times) : EventBase
{
    public long OperatorUin { get; } = operatorUin;
    public string Nickname { get; } = nickname;
    public int Times { get; } = times;

    public override string ToEventMessage() => $"{nameof(BotFriendProfileLikeEvent)}: OperatorUin={OperatorUin}, Times={Times}";
}
