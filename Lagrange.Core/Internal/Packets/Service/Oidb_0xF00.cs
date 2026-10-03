using Lagrange.Proto;
namespace Lagrange.Core.Internal.Packets.Service;
#pragma warning disable CS8618
[ProtoPackable] internal partial class F00ReqBody { [ProtoMember(1)] public long GroupCode { get; set; } [ProtoMember(2)] public F00GroupInfo Info { get; set; } }
[ProtoPackable] internal partial class F00GroupInfo { [ProtoMember(1)] public long GroupCode { get; set; } [ProtoMember(2)] public F00ExtInfo Ext { get; set; } }
[ProtoPackable] internal partial class F00ExtInfo { [ProtoMember(29)] public uint? InviteRobotMemberSwitch { get; set; } [ProtoMember(30)] public uint? InviteRobotMemberExamine { get; set; } }
[ProtoPackable] internal partial class F00RspBody { [ProtoMember(1)] public long GroupCode { get; set; } [ProtoMember(2)] public int Result { get; set; } }
