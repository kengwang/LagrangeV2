using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class D1096ReqBody
{
    [ProtoMember(1)] public ulong GroupUin { get; set; }
    [ProtoMember(2)] public string Uid { get; set; }
    [ProtoMember(3)] public bool IsAdmin { get; set; }
}

[ProtoPackable]
internal partial class D1096RespBody { }
