namespace Lagrange.Core.Internal.Events.System;

internal class FriendLikeEventReq(string targetUid, uint count) : ProtocolEvent
{
    public string TargetUid { get; } = targetUid;

    public uint Count { get; } = count;
}

internal class FriendLikeEventResp : ProtocolEvent
{
    public static readonly FriendLikeEventResp Default = new();
}

internal class DeleteFriendEventReq(string targetUid) : ProtocolEvent
{
    public string TargetUid { get; } = targetUid;
}

internal class DeleteFriendEventResp : ProtocolEvent
{
    public static readonly DeleteFriendEventResp Default = new();
}
