using Lagrange.Core.Common.Response;
using Lagrange.Core.Internal.Events.System;
namespace Lagrange.Core.Common.Interface;
/// <summary>Media download credential operations.</summary>
public static class DownloadRKeyExt
{
    /// <summary>Fetches private, group and fallback download credentials.</summary>
    public static async Task<IReadOnlyList<BotDownloadRKey>> GetDownloadRKeys(this BotContext context, CancellationToken ct = default) => (await context.EventContext.SendEvent<DownloadRKeyEventResp>(new DownloadRKeyEventReq(), ct)).Keys;
}
