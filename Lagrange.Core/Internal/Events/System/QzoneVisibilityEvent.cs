using Lagrange.Core.Events;
namespace Lagrange.Core.Internal.Events.System;
internal sealed class QzoneVisibilityEventReq(string messageId, uint right, IReadOnlyList<long> users) : ProtocolEvent
{
    public string MessageId { get; } = messageId;
    public uint Right { get; } = right;
    public IReadOnlyList<long> Users { get; } = users;
}
internal sealed class QzoneVisibilityEventResp : ProtocolEvent;
