namespace Lagrange.Core.Internal.Events.System;

internal class FetchCustomFaceEventReq : ProtocolEvent;

internal class FetchCustomFaceEventResp(IReadOnlyList<string> urls) : ProtocolEvent
{
    public IReadOnlyList<string> Urls { get; } = urls;
}
