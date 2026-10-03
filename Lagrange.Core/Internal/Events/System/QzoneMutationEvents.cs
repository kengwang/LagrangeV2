using Lagrange.Core.Common.Response;
using Lagrange.Core.Events;

namespace Lagrange.Core.Internal.Events.System;

internal sealed class SetQzoneLikeEventReq(long targetUin, string messageId, bool like, long abstime) : ProtocolEvent { public long TargetUin { get; } = targetUin; public string MessageId { get; } = messageId; public bool Like { get; } = like; public long AbsTime { get; } = abstime; }
internal sealed class SetQzoneLikeEventResp : ProtocolEvent { public static SetQzoneLikeEventResp Instance { get; } = new(); }
internal enum QzoneMutationKind { Comment, Delete, Publish, Black }
internal sealed class QzoneMutationEventReq(QzoneMutationKind kind, long targetUin, string messageId, string content, string richValue, bool ban) : ProtocolEvent
{
    public QzoneMutationKind Kind { get; } = kind; public long TargetUin { get; } = targetUin; public string MessageId { get; } = messageId; public string Content { get; } = content; public string RichValue { get; } = richValue; public bool Ban { get; } = ban;
}
internal sealed class QzoneMutationEventResp(BotQzoneCommentResult? comment, BotQzonePublishResult? publish) : ProtocolEvent
{
    public BotQzoneCommentResult? Comment { get; } = comment; public BotQzonePublishResult? Publish { get; } = publish;
}
