using Lagrange.Core.Events;

namespace Lagrange.Core.Internal.Events.System;

internal sealed class SetGroupMemberPermissionEventReq(long groupUin, uint privilegeFlag, uint privilegeMask) : ProtocolEvent
{
    public long GroupUin { get; } = groupUin;
    public uint PrivilegeFlag { get; } = privilegeFlag;
    public uint PrivilegeMask { get; } = privilegeMask;
}

internal sealed class SetGroupMemberPermissionEventResp : ProtocolEvent;
