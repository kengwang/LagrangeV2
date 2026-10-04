using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Internal.Logic;
using Lagrange.Core.Message;
namespace Lagrange.Core.Common.Interface;
/// <summary>Extended message and session operations.</summary>
public static class ExtendedMessageExt
{
    /// <summary>Sends a message through a group temporary session.</summary>
    /// <param name="context">Bot session.</param><param name="groupUin">Source group identifier.</param>
    /// <param name="userUin">Recipient identifier.</param><param name="chain">Message elements.</param><param name="ct">Cancellation.</param>
    /// <returns>The message with its acknowledged sequence and send time.</returns>
    public static Task<BotMessage> SendTempMessage(this BotContext context, long groupUin, long userUin, MessageChain chain, CancellationToken ct = default) => context.EventContext.GetLogic<MessagingLogic>().SendTempMessage(groupUin, userUin, chain, ct);
    /// <summary>Returns the most recent device snapshot, or null until a snapshot arrives in this session.</summary>
    /// <param name="context">Bot session.</param><returns>A copy of the latest device snapshot, or null when unknown.</returns>
    public static IReadOnlyList<BotOnlineDevice>? GetOnlineDevices(this BotContext context) => context.CacheContext.OnlineDevices;
}
