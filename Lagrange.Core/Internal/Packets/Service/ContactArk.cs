using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class D12B6Req
{
    [ProtoMember(1)] public ulong Uin { get; set; }
    [ProtoMember(2)] public string Phone { get; set; }
    [ProtoMember(3)] public string JumpUrl { get; set; }
}

[ProtoPackable]
internal partial class D12B6Resp
{
    [ProtoMember(1)] public string Ark { get; set; }
}

[ProtoPackable]
internal partial class D8B7Req
{
    [ProtoMember(1)] public uint RequestType { get; set; }
    [ProtoMember(2)] public ulong GroupCode { get; set; }
    [ProtoMember(5)] public uint Flag { get; set; }
}

[ProtoPackable]
internal partial class D8B7Resp
{
    [ProtoMember(1)] public uint ErrorCode { get; set; }
    [ProtoMember(5)] public string ArkJson { get; set; }
}
