using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class DE17ReqBody
{
    [ProtoMember(2)] public uint? SubCommand { get; set; } = 0;
    [ProtoMember(3)] public string JsonBody { get; set; }
}

[ProtoPackable]
internal partial class DE17RespBody
{
    [ProtoMember(3)] public int ErrorCode { get; set; }
    [ProtoMember(5)] public string? ErrorMessage { get; set; }
    [ProtoMember(4)] public string? JsonBody { get; set; }
}
