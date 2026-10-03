using Lagrange.Core.Events;

namespace Lagrange.Core.Internal.Events.System;

internal sealed class SetGroupAddOptionEventReq(long groupUin, uint addType, string question, string answer) : ProtocolEvent
{
    public long GroupUin { get; } = groupUin;
    public uint AddType { get; } = addType;
    public string Question { get; } = question;
    public string Answer { get; } = answer;
}
internal sealed class SetGroupAddOptionEventResp : ProtocolEvent { public static readonly SetGroupAddOptionEventResp Default = new(); }

internal sealed class SetGroupSearchEventReq(long groupUin, uint? noFingerOpen, uint? noCodeFingerOpen) : ProtocolEvent
{
    public long GroupUin { get; } = groupUin;
    public uint? NoFingerOpen { get; } = noFingerOpen;
    public uint? NoCodeFingerOpen { get; } = noCodeFingerOpen;
}
internal sealed class SetGroupSearchEventResp : ProtocolEvent { public static readonly SetGroupSearchEventResp Default = new(); }

internal sealed class SetGroupNewMemberHistoryEventReq(long groupUin, bool visible) : ProtocolEvent
{
    public long GroupUin { get; } = groupUin;
    public bool Visible { get; } = visible;
}
internal sealed class SetGroupNewMemberHistoryEventResp : ProtocolEvent { public static readonly SetGroupNewMemberHistoryEventResp Default = new(); }

internal sealed class SetGroupInvitePolicyEventReq(long groupUin, uint privilegeFlag, string policy) : ProtocolEvent
{
    public long GroupUin { get; } = groupUin;
    public uint PrivilegeFlag { get; } = privilegeFlag;
    public string Policy { get; } = policy;
}
internal sealed class SetGroupInvitePolicyEventResp : ProtocolEvent { public static readonly SetGroupInvitePolicyEventResp Default = new(); }
