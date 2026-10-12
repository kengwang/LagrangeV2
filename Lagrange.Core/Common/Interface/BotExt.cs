using Lagrange.Core.Common.Entity;
using Lagrange.Core.Internal.Logic;

namespace Lagrange.Core.Common.Interface;

public static class BotExt
{
    public static Task<bool> Login(this BotContext context, long uin, string password, CancellationToken token = default) =>
        context.EventContext.GetLogic<WtExchangeLogic>().Login(uin, password, token);
    
    public static Task<bool> Login(this BotContext context, CancellationToken token = default) =>
        context.EventContext.GetLogic<WtExchangeLogic>().Login(0, null, token);

    public static Task<bool> Logout(this BotContext context) =>
        context.EventContext.GetLogic<WtExchangeLogic>().Logout();
    
    public static Task<long> ResolveUinByQid(this BotContext context, string qid) =>
        context.EventContext.GetLogic<WtExchangeLogic>().ResolveUinByQid(qid);

    public static bool SubmitCaptcha(this BotContext context, string ticket, string randStr) =>
        context.EventContext.GetLogic<WtExchangeLogic>().SubmitCaptcha(ticket, randStr);

    public static bool SubmitSMSCode(this BotContext context, string code) =>
        context.EventContext.GetLogic<WtExchangeLogic>().SubmitSMSCode(code);

    public static ValueTask<BotSsoPacket> SendPacket(this BotContext context, BotSsoPacket packet, RequestType requestType = RequestType.D2Auth, EncryptType encryptType = EncryptType.EncryptD2Key)
        => context.EventContext.GetLogic<OperationLogic>().SendPacket(packet, requestType, encryptType);

    /// <summary>Sends an arbitrary SSO payload. Nonzero response codes are returned unchanged.
    /// With <paramref name="waitForResponse"/> disabled, returns null after writing the complete frame.
    /// Cancellation does not retract a packet already sent and never retries it.</summary>
    /// <param name="context">The bot whose authenticated connection is used.</param>
    /// <param name="packet">The command and arbitrary binary payload to send.</param>
    /// <param name="waitForResponse">Whether to wait for a matching SSO response.</param>
    /// <param name="cancellationToken">Cancels waiting for signing, socket writes and the response.</param>
    /// <returns>The raw response, or null after a completed send when no response is requested.</returns>
    public static ValueTask<BotSsoPacket?> SendRawPacket(this BotContext context, BotSsoPacket packet,
        bool waitForResponse = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(packet);
        ArgumentException.ThrowIfNullOrWhiteSpace(packet.Command);
        cancellationToken.ThrowIfCancellationRequested();
        // A new sequence for every raw operation, including reuse of the same packet instance.
        var outgoing = new BotSsoPacket(packet.Command, packet.Data, context.ServiceContext.GetNewSequence());
        return context.PacketContext.SendRawPacket(outgoing, waitForResponse, cancellationToken);
    }
}
