using Lagrange.Core.Common.Entity;
using Lagrange.Core.Events;

namespace Lagrange.Core.Internal.Events.System;

internal sealed class FetchDoubtFriendRequestsEventReq : ProtocolEvent;
internal sealed class FetchDoubtFriendRequestsEventResp(IReadOnlyList<BotDoubtFriendRequest> requests) : ProtocolEvent
{
    public IReadOnlyList<BotDoubtFriendRequest> Requests { get; } = requests;
}

internal sealed class HandleDoubtApprovalEventReq(string uid) : ProtocolEvent { public string Uid { get; } = uid; }
internal sealed class HandleDoubtApprovalEventResp : ProtocolEvent { public static HandleDoubtApprovalEventResp Instance { get; } = new(); }
internal sealed class HandleDoubtDeleteEventReq(string uid) : ProtocolEvent { public string Uid { get; } = uid; }
internal sealed class HandleDoubtDeleteEventResp : ProtocolEvent { public static HandleDoubtDeleteEventResp Instance { get; } = new(); }

