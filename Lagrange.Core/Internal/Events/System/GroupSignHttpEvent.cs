using Lagrange.Core.Common.Response;
using Lagrange.Core.Events;

namespace Lagrange.Core.Internal.Events.System;

internal sealed class GetGroupSignInEventReq(long groupUin, DateTime? day) : ProtocolEvent { public long GroupUin { get; } = groupUin; public DateTime? Day { get; } = day; }
internal sealed class GetGroupSignInEventResp(BotGroupSignInResult result) : ProtocolEvent { public BotGroupSignInResult Result { get; } = result; }
