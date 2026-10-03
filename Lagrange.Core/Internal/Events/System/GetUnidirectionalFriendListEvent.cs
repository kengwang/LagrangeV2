using Lagrange.Core.Common.Response;
using Lagrange.Core.Events;

namespace Lagrange.Core.Internal.Events.System;

internal class GetUnidirectionalFriendListEventReq : ProtocolEvent;

internal class GetUnidirectionalFriendListEventResp(BotUnidirectionalFriendResult result) : ProtocolEvent
{
    public BotUnidirectionalFriendResult Result { get; } = result;
}
