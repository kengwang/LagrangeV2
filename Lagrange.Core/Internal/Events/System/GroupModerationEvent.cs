namespace Lagrange.Core.Internal.Events.System;

internal class GroupSetAdminEventReq(long groupUin, string uid, bool isAdmin) : ProtocolEvent
{
    public long GroupUin { get; } = groupUin;

    public string Uid { get; } = uid;

    public bool IsAdmin { get; } = isAdmin;
}

internal class GroupSetAdminEventResp : ProtocolEvent
{
    public static readonly GroupSetAdminEventResp Default = new();
}

internal class GroupMuteMemberEventReq(long groupUin, string uid, uint duration) : ProtocolEvent
{
    public long GroupUin { get; } = groupUin;

    public string Uid { get; } = uid;

    public uint Duration { get; } = duration;
}

internal class GroupMuteMemberEventResp : ProtocolEvent
{
    public static readonly GroupMuteMemberEventResp Default = new();
}

internal class GroupMuteGlobalEventReq(long groupUin, bool isMute) : ProtocolEvent
{
    public long GroupUin { get; } = groupUin;

    public bool IsMute { get; } = isMute;
}

internal class GroupMuteGlobalEventResp : ProtocolEvent
{
    public static readonly GroupMuteGlobalEventResp Default = new();
}

internal class GroupKickMemberEventReq(long groupUin, string uid, bool rejectAddRequest) : ProtocolEvent
{
    public long GroupUin { get; } = groupUin;

    public string Uid { get; } = uid;

    public bool RejectAddRequest { get; } = rejectAddRequest;
}

internal class GroupKickMemberEventResp : ProtocolEvent
{
    public static readonly GroupKickMemberEventResp Default = new();
}
