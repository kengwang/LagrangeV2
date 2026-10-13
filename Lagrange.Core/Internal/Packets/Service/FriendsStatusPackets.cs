using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

[ProtoPackable]
internal partial class FriendsStatusRequest
{
    [ProtoMember(1)] public string SelfUid { get; set; } = string.Empty;
    [ProtoMember(5)] public uint Field5 { get; set; } = 1;
    [ProtoMember(7)] public uint Field7 { get; set; }
    [ProtoMember(8)] public uint Field8 { get; set; } = 1;
    [ProtoMember(9)] public uint Field9 { get; set; }
    [ProtoMember(10)] public uint Field10 { get; set; }
    [ProtoMember(100)] public uint Field100 { get; set; }
    [ProtoMember(101)] public uint Field101 { get; set; } = 400;
}

[ProtoPackable]
internal partial class FriendsStatusResponse
{
    [ProtoMember(1)] public string SelfUid { get; set; } = string.Empty;
    // Preserve complete entry bytes so unknown and repeated fields are not discarded.
    [ProtoMember(5)] public List<ReadOnlyMemory<byte>> Friends { get; set; } = [];
}
