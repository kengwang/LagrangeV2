using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class D912EReqBody
{
    [ProtoMember(1)] public D912EChange Change { get; set; }
    [ProtoMember(2)] public uint Scene { get; set; }
}

[ProtoPackable]
internal partial class D912EChange
{
    [ProtoMember(1)] public D912ETarget Target { get; set; }
    [ProtoMember(2)] public string Remark { get; set; }
}

[ProtoPackable]
internal partial class D912ETarget
{
    [ProtoMember(3)] public ulong TargetUin { get; set; }
    [ProtoMember(7)] public string TargetUid { get; set; }
}

[ProtoPackable]
internal partial class D912ERspBody
{
    [ProtoMember(1)] public D912EChange? Result { get; set; }
    [ProtoMember(3)] public D912EError? Error { get; set; }
}

[ProtoPackable]
internal partial class D912EError
{
    [ProtoMember(2)] public int Code { get; set; }
    [ProtoMember(3)] public string Message { get; set; }
}
