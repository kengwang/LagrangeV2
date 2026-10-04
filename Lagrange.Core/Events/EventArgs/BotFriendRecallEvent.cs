namespace Lagrange.Core.Events.EventArgs;

public class BotFriendRecallEvent(long peerUin, long authorUin, ulong sequence, string tip, bool recalledBySelf = false, string peerUid = "", long time = 0) : EventBase
{
    public long PeerUin { get; } = peerUin;

    public long AuthorUin { get; } = authorUin;

    public ulong Sequence { get; } = sequence;

    public string Tip { get; } = tip;
    public bool RecalledBySelf { get; } = recalledBySelf;
    public string PeerUid { get; } = peerUid;
    public long Time { get; } = time;

    public override string ToEventMessage()
    {
        return $"{nameof(BotGroupRecallEvent)}: ${AuthorUin} recalled {Sequence} in {PeerUin}";
    }
}
