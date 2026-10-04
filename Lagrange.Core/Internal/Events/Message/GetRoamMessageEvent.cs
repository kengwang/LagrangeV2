using Lagrange.Core.Events;
using Lagrange.Core.Internal.Packets.Message;

namespace Lagrange.Core.Internal.Events.Message;

internal class GetRoamMessageEventReq(string peerUid, uint time, uint count, uint random = 0, uint direction = 2) : ProtocolEvent
{
    public string PeerUid { get; } = peerUid;

    public uint Time { get; } = time;

    public uint Count { get; } = count;
    public uint Random { get; } = random;
    public uint Direction { get; } = direction;
}

internal class GetRoamMessageEventResp(List<CommonMessage> chains, uint time = 0, uint random = 0, bool isComplete = false, string? peerUid = null) : ProtocolEvent
{
    public List<CommonMessage> Chains { get; } = chains;
    public uint Time { get; } = time;
    public uint Random { get; } = random;
    public bool IsComplete { get; } = isComplete;
    public string? PeerUid { get; } = peerUid;
}
