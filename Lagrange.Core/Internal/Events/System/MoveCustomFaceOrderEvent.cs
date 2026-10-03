using Lagrange.Core.Events;

namespace Lagrange.Core.Internal.Events.System;

internal class MoveCustomFaceOrderEventReq(IReadOnlyList<CustomFaceLookup> entries) : ProtocolEvent
{
    public IReadOnlyList<CustomFaceLookup> Entries { get; } = entries;
}

internal class MoveCustomFaceOrderEventResp : ProtocolEvent
{
    public static readonly MoveCustomFaceOrderEventResp Default = new();
}
