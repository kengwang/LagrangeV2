using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Message;

[ProtoPackable]
internal partial class SsoReadedReportReq
{
    [ProtoMember(1)] public List<GroupReadedReportItem>? GroupList { get; set; }
    [ProtoMember(2)] public List<C2CReadedReportItem>? C2CList { get; set; }
}

[ProtoPackable]
internal partial class GroupReadedReportItem { [ProtoMember(1)] public ulong GroupUin { get; set; } [ProtoMember(2)] public ulong? LastReadSeq { get; set; } }
[ProtoPackable]
internal partial class C2CReadedReportItem { [ProtoMember(2)] public string Uid { get; set; } = string.Empty; [ProtoMember(3)] public ulong? LastReadTime { get; set; } [ProtoMember(4)] public ulong? LastReadSeq { get; set; } }

[ProtoPackable]
internal partial class SsoReadedReportResp
{
    [ProtoMember(1)] public uint ResultCode { get; set; }
    [ProtoMember(2)] public string? ErrorMessage { get; set; }
    [ProtoMember(3)] public List<GroupReadedReportResponseItem>? GroupList { get; set; }
    [ProtoMember(4)] public List<C2CReadedReportResponseItem>? C2CList { get; set; }
}
[ProtoPackable]
internal partial class GroupReadedReportResponseItem { [ProtoMember(1)] public uint ResultCode { get; set; } [ProtoMember(2)] public string? ErrorMessage { get; set; } [ProtoMember(3)] public ulong GroupUin { get; set; } [ProtoMember(4)] public ulong ReadSeq { get; set; } [ProtoMember(5)] public ulong LatestSeq { get; set; } }
[ProtoPackable]
internal partial class C2CReadedReportResponseItem { [ProtoMember(1)] public uint ResultCode { get; set; } [ProtoMember(2)] public string? ErrorMessage { get; set; } [ProtoMember(3)] public ulong TargetUin { get; set; } [ProtoMember(4)] public string? Uid { get; set; } [ProtoMember(5)] public ulong ReadSeq { get; set; } [ProtoMember(6)] public ulong LatestSeq { get; set; } [ProtoMember(7)] public ulong LastMsgTime { get; set; } }
