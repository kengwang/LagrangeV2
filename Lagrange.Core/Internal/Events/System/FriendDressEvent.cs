using Lagrange.Core.Common.Response;
using Lagrange.Core.Events;

namespace Lagrange.Core.Internal.Events.System;

internal sealed class GetFriendDressEventReq(long targetUin) : ProtocolEvent { public long TargetUin { get; } = targetUin; }
internal sealed class GetFriendDressEventResp(BotFriendDressResult result) : ProtocolEvent { public BotFriendDressResult Result { get; } = result; }
