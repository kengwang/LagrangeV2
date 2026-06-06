using Lagrange.Core.Common.Entity;

namespace Lagrange.Core.Internal.Events.System;

internal class FetchFriendRequestsEventReq : ProtocolEvent;

internal class FetchFriendRequestsEventResp(IReadOnlyList<BotFriendRequest> requests) : ProtocolEvent
{
    public IReadOnlyList<BotFriendRequest> Requests { get; } = requests;
}

internal class SetFriendRequestEventReq(string targetUid, bool accept) : ProtocolEvent
{
    public string TargetUid { get; } = targetUid;

    public bool Accept { get; } = accept;
}

internal class SetFriendRequestEventResp : ProtocolEvent
{
    public static readonly SetFriendRequestEventResp Default = new();
}
