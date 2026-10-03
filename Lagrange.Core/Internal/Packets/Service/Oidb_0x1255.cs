using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class D1255ReqBody
{
    [ProtoMember(1)] public string Uid { get; set; }
    [ProtoMember(2)] public uint CategoryId { get; set; }
}

[ProtoPackable]
internal partial class D1255RespBody { }
