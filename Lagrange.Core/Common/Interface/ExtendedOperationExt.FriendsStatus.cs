using Lagrange.Core.Common.Response;
using Lagrange.Core.Internal.Events.System;

namespace Lagrange.Core.Common.Interface;

public static partial class ExtendedOperationExt
{
    /// <summary>Requests the status list returned by QQ for the current session's UID.</summary>
    /// <param name="context">The authenticated bot session.</param>
    /// <param name="ct">Cancels the request.</param>
    /// <returns>A single server response, including self and duplicate entries when present.</returns>
    public static async Task<BotFriendsStatusResult> GetFriendsStatus(this BotContext context, CancellationToken ct = default)
        => (await context.EventContext.SendEvent<GetFriendsStatusEventResp>(new GetFriendsStatusEventReq(), ct)).Result;
}
