using Lagrange.Core.Internal.Events.System;
namespace Lagrange.Core.Common.Interface;
/// <summary>Visibility operations for existing Qzone posts.</summary>
public static class QzoneVisibilityExt
{
    /// <summary>Changes visibility while preserving the existing post's text and pictures.</summary>
    /// <param name="context">The authenticated account.</param><param name="messageId">Post identifier.</param>
    /// <param name="right">1 public, 4 friends, 16 selected users, 64 self, 128 excluded users.</param>
    /// <param name="users">Selected or excluded QQ numbers.</param><param name="ct">Cancellation token.</param>
    public static async Task SetQzoneMessageVisibility(this BotContext context, string messageId, uint right, IReadOnlyList<long>? users = null, CancellationToken ct = default) =>
        await context.EventContext.SendEvent<QzoneVisibilityEventResp>(new QzoneVisibilityEventReq(messageId, right, users ?? []), ct);
}
