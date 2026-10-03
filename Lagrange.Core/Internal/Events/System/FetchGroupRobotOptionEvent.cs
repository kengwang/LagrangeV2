using Lagrange.Core.Events;
namespace Lagrange.Core.Internal.Events.System;
internal sealed class FetchGroupRobotOptionEventReq(long groupUin) : ProtocolEvent { public long GroupUin { get; } = groupUin; }
internal sealed class FetchGroupRobotOptionEventResp(uint memberSwitch, uint memberExamine) : ProtocolEvent { public uint MemberSwitch { get; } = memberSwitch; public uint MemberExamine { get; } = memberExamine; }
