using System.Text.Json;
using Lagrange.Core.Internal.Events.System;

namespace Lagrange.Core.Common.Interface;

public static class QzoneWebExt
{
    public static async Task<JsonElement> GetQzoneProfile(this BotContext context, long? targetUin = null, CancellationToken ct = default) =>
        (await context.EventContext.SendEvent<QzoneRpcResult>(new QzoneWebEvent(QzoneWebOperation.GetProfile) { TargetUin = targetUin ?? context.BotUin }, ct)).Data;

    public static async Task SetQzoneProfile(this BotContext context, string? name = null, string? description = null, CancellationToken ct = default) =>
        await context.EventContext.SendEvent<QzoneRpcResult>(new QzoneWebEvent(QzoneWebOperation.SetProfile) { Name = name, Description = description }, ct);

    public static async Task<JsonElement> GetQzoneMessageDetail(this BotContext context, long targetUin, string messageId, int position = 0, int count = 20, CancellationToken ct = default) =>
        (await context.EventContext.SendEvent<QzoneRpcResult>(new QzoneWebEvent(QzoneWebOperation.GetDetail) { TargetUin = targetUin, PostId = messageId, Position = position, Count = count }, ct)).Data;

    public static async Task<JsonElement> ReplyQzoneComment(this BotContext context, long targetUin, string messageId, string content, string commentId, long commentUin,
        long? replyUin = null, string? replyName = null, CancellationToken ct = default) =>
        (await context.EventContext.SendEvent<QzoneRpcResult>(new QzoneWebEvent(QzoneWebOperation.Reply) { TargetUin = targetUin, PostId = messageId, Content = content,
            CommentId = commentId, CommentUin = commentUin, ReplyUin = replyUin, ReplyName = replyName }, ct)).Data;
}
