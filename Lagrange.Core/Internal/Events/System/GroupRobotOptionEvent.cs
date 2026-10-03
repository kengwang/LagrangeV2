using Lagrange.Core.Events;
namespace Lagrange.Core.Internal.Events.System;
internal sealed class SetGroupRobotOptionEventReq(long groupUin, uint? memberSwitch, uint? memberExamine) : ProtocolEvent { public long GroupUin { get; } = groupUin; public uint? MemberSwitch { get; } = memberSwitch; public uint? MemberExamine { get; } = memberExamine; }
internal sealed class SetGroupRobotOptionEventResp : ProtocolEvent { public static readonly SetGroupRobotOptionEventResp Default = new(); }
