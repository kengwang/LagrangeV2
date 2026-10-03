using Lagrange.Core.Common.Response;
using Lagrange.Core.Events;

namespace Lagrange.Core.Internal.Events.System;

internal sealed class GetProfileLikeEventReq(long? userUin, int start, int limit) : ProtocolEvent
{
    public long? UserUin { get; } = userUin;
    public int Start { get; } = start;
    public int Limit { get; } = limit;
}

internal sealed class GetProfileLikeEventResp(BotProfileLikeResult result) : ProtocolEvent
{
    public BotProfileLikeResult Result { get; } = result;
}
