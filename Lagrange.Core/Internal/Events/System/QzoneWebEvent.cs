using Lagrange.Core.Events;

namespace Lagrange.Core.Internal.Events.System;

internal enum QzoneWebOperation { GetProfile, SetProfile, GetDetail, Reply }
internal sealed class QzoneWebEvent(QzoneWebOperation operation) : ProtocolEvent
{
    public QzoneWebOperation Operation { get; } = operation;
    public long TargetUin { get; init; }
    public string PostId { get; init; } = string.Empty;
    public int Position { get; init; }
    public int Count { get; init; } = 20;
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string CommentId { get; init; } = string.Empty;
    public long CommentUin { get; init; }
    public string Content { get; init; } = string.Empty;
    public long? ReplyUin { get; init; }
    public string? ReplyName { get; init; }
}
