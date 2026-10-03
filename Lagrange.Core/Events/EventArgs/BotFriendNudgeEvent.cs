namespace Lagrange.Core.Events.EventArgs;

public sealed class BotFriendNudgeEvent(long peerUin, long senderUin, long targetUin, string action, string actionImgUrl, string suffix) : EventBase
{
    public long PeerUin { get; } = peerUin;
    public long SenderUin { get; } = senderUin;
    public long TargetUin { get; } = targetUin;
    public string Action { get; } = action;
    public string ActionImageUrl { get; } = actionImgUrl;
    public string Suffix { get; } = suffix;
    public override string ToEventMessage() => $"{nameof(BotFriendNudgeEvent)}: {SenderUin} nudged {TargetUin}";
}
