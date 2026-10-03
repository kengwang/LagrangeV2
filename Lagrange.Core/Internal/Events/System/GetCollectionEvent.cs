using Lagrange.Core.Common.Response;
using Lagrange.Core.Events;

namespace Lagrange.Core.Internal.Events.System;

internal sealed class GetCollectionEventReq(uint count) : ProtocolEvent
{
    public uint Count { get; } = count;
}

internal sealed class GetCollectionEventResp(BotCollectionResult result) : ProtocolEvent
{
    public BotCollectionResult Result { get; } = result;
}
