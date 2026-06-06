using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

[ProtoPackable]
internal partial class EssenceMessageRequest
{
    [ProtoMember(1)] public long GroupUin { get; set; }

    [ProtoMember(2)] public ulong Sequence { get; set; }

    [ProtoMember(3)] public uint Random { get; set; }
}

[ProtoPackable]
internal partial class EssenceMessageResponse;
