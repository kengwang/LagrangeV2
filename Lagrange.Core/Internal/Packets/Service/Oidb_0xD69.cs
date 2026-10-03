using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class D69GetReq
{
    [ProtoMember(1)] public uint Operation { get; set; }
    [ProtoMember(2)] public D69GetReqInner Inner { get; set; }
}

[ProtoPackable]
internal partial class D69GetReqInner
{
    [ProtoMember(1)] public uint Count { get; set; }
    [ProtoMember(2)] public string Cookie { get; set; }
}

[ProtoPackable]
internal partial class D69GetResp
{
    [ProtoMember(1)] public uint Status { get; set; }
    [ProtoMember(2)] public D69GetRespBody Body { get; set; }
}

[ProtoPackable]
internal partial class D69GetRespBody
{
    [ProtoMember(1)] public List<D69DoubtItem> Items { get; set; }
    [ProtoMember(2)] public string Reason { get; set; }
}

[ProtoPackable]
internal partial class D69DoubtItem
{
    [ProtoMember(1)] public string Uid { get; set; }
    [ProtoMember(2)] public string Nick { get; set; }
    [ProtoMember(3)] public uint Age { get; set; }
    [ProtoMember(4)] public uint Sex { get; set; }
    [ProtoMember(5)] public string Message { get; set; }
    [ProtoMember(6)] public string Source { get; set; }
    [ProtoMember(7)] public string Reason { get; set; }
    [ProtoMember(8)] public ulong Uin { get; set; }
    [ProtoMember(9)] public ulong RequestTime { get; set; }
    [ProtoMember(10)] public uint CommonFriendCount { get; set; }
    [ProtoMember(11)] public string GroupCode { get; set; }
}

[ProtoPackable]
internal partial class D69ApproveReq
{
    [ProtoMember(1)] public string Uid { get; set; }
    [ProtoMember(2)] public string TargetUid { get; set; }
}

[ProtoPackable]
internal partial class D69DeleteReq
{
    [ProtoMember(1)] public uint Operation { get; set; }
    [ProtoMember(3)] public D69DeleteReqInner Inner { get; set; }
}

[ProtoPackable]
internal partial class D69DeleteReqInner
{
    [ProtoMember(1)] public string Uid { get; set; }
}
