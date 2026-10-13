using Lagrange.Core.Common.Response;
using Lagrange.Core.Events;

namespace Lagrange.Core.Internal.Events.System;

internal sealed class GetFriendsStatusEventReq : ProtocolEvent;

internal sealed class GetFriendsStatusEventResp(BotFriendsStatusResult result) : ProtocolEvent
{
    public BotFriendsStatusResult Result { get; } = result;
}
