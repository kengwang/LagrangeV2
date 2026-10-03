using Lagrange.Core.Events;

namespace Lagrange.Core.Internal.Events.Message;

internal sealed class MarkMessageReadEventReq(string scene, long peerUin, ulong lastReadSeq) : ProtocolEvent
{
    public string Scene { get; } = scene;
    public long PeerUin { get; } = peerUin;
    public ulong LastReadSeq { get; } = lastReadSeq;
}

internal sealed class MarkMessageReadEventResp : ProtocolEvent;

internal sealed class MarkAllMessagesReadEventReq(IReadOnlyList<long> groupUins, IReadOnlyList<long> privateUins) : ProtocolEvent
{
    public IReadOnlyList<long> GroupUins { get; } = groupUins;
    public IReadOnlyList<long> PrivateUins { get; } = privateUins;
}

internal sealed class MarkAllMessagesReadEventResp(IReadOnlyList<(long PeerUin, ulong LatestSeq)> groups, IReadOnlyList<(long PeerUin, ulong LatestSeq)> privates) : ProtocolEvent
{
    public IReadOnlyList<(long PeerUin, ulong LatestSeq)> Groups { get; } = groups;
    public IReadOnlyList<(long PeerUin, ulong LatestSeq)> Privates { get; } = privates;
}
