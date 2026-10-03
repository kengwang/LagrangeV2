using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class DCD4ReqBody
{
    [ProtoMember(1)] public string Uid { get; set; }
    [ProtoMember(2)] public uint ChatType { get; set; }
    [ProtoMember(3)] public uint EventType { get; set; }
}

[ProtoPackable]
internal partial class DCD4Req
{
    [ProtoMember(1)] public DCD4ReqBody ReqBody { get; set; }
}

[ProtoPackable]
internal partial class DCD4RespBody { }
