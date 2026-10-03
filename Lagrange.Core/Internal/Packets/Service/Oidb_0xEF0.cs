using Lagrange.Proto;
namespace Lagrange.Core.Internal.Packets.Service;
#pragma warning disable CS8618
[ProtoPackable] internal partial class EF0ReqBody { [ProtoMember(1)] public List<ulong> GroupCodes { get; set; } [ProtoMember(2)] public EF0Filter Filter { get; set; } }
[ProtoPackable] internal partial class EF0Filter { [ProtoMember(29)] public uint? InviteRobotMemberSwitch { get; set; } [ProtoMember(30)] public uint? InviteRobotMemberExamine { get; set; } }
[ProtoPackable] internal partial class EF0RspBody { [ProtoMember(1)] public List<EF0Item> Items { get; set; } }
[ProtoPackable] internal partial class EF0Item { [ProtoMember(1)] public ulong GroupCode { get; set; } [ProtoMember(2)] public uint ResultCode { get; set; } [ProtoMember(3)] public EF0Ext Ext { get; set; } }
[ProtoPackable] internal partial class EF0Ext { [ProtoMember(29)] public uint InviteRobotMemberSwitch { get; set; } [ProtoMember(30)] public uint InviteRobotMemberExamine { get; set; } }
