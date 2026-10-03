using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class D7E5ReqBody
{
    [ProtoMember(11)] public string TargetUid { get; set; }
    [ProtoMember(12)] public uint SourceId { get; set; }
    [ProtoMember(13)] public uint Count { get; set; }
}

[ProtoPackable]
internal partial class D7E5RespBody { }
