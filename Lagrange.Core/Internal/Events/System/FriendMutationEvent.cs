using Lagrange.Core.Events;

namespace Lagrange.Core.Internal.Events.System;

internal class DeleteFriendEventReq(long userUin, bool block) : ProtocolEvent
{
    public long UserUin { get; } = userUin;
    public bool Block { get; } = block;
}

internal class DeleteFriendEventResp : ProtocolEvent
{
    public static readonly DeleteFriendEventResp Default = new();
}

internal class SetFriendRemarkEventReq(long userUin, string remark) : ProtocolEvent
{
    public long UserUin { get; } = userUin;
    public string Remark { get; } = remark;
}

internal class SetFriendRemarkEventResp : ProtocolEvent
{
    public static readonly SetFriendRemarkEventResp Default = new();
}

internal class SetGroupAdminEventReq(long groupUin, long userUin, bool enable) : ProtocolEvent
{
    public long GroupUin { get; } = groupUin;
    public long UserUin { get; } = userUin;
    public bool Enable { get; } = enable;
}

internal class SetGroupAdminEventResp : ProtocolEvent
{
    public static readonly SetGroupAdminEventResp Default = new();
}

internal class HandleFriendRequestEventReq(string uidOrFlag, bool approve) : ProtocolEvent
{
    public string UidOrFlag { get; } = uidOrFlag;
    public bool Approve { get; } = approve;
}

internal class HandleFriendRequestEventResp : ProtocolEvent
{
    public static readonly HandleFriendRequestEventResp Default = new();
}

internal class SetFriendCategoryEventReq(long userUin, int categoryId) : ProtocolEvent
{
    public long UserUin { get; } = userUin;
    public int CategoryId { get; } = categoryId;
}

internal class SetFriendCategoryEventResp : ProtocolEvent
{
    public static readonly SetFriendCategoryEventResp Default = new();
}
