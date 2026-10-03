using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class DE17ReqBody
{
    [ProtoMember(3)] public string JsonBody { get; set; }
}

[ProtoPackable]
internal partial class DE17RespBody
{
    [ProtoMember(4)] public string? JsonBody { get; set; }
}
