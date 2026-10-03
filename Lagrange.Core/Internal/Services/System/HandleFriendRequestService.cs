using System.Globalization;
using Lagrange.Core.Common;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<HandleFriendRequestEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0xb5d_44")]
internal sealed class HandleFriendRequestService : OidbService<HandleFriendRequestEventReq, HandleFriendRequestEventResp, DB5DReqBody, DB5DRespBody>
{
    protected override uint Command => 0xb5d;
    protected override uint Service => 44;

    protected override async Task<DB5DReqBody> ProcessRequest(HandleFriendRequestEventReq request, BotContext context)
    {
        string uid = request.UidOrFlag;
        if (ulong.TryParse(uid, NumberStyles.None, CultureInfo.InvariantCulture, out ulong uin))
        {
            var friend = await context.CacheContext.ResolveStranger((long)uin);
            uid = friend.Uid;
        }

        return new DB5DReqBody
        {
            Accept = request.Approve ? 3u : 5u,
            TargetUid = uid,
        };
    }

    protected override Task<HandleFriendRequestEventResp> ProcessResponse(DB5DRespBody response, BotContext context) =>
        Task.FromResult(HandleFriendRequestEventResp.Default);
}
