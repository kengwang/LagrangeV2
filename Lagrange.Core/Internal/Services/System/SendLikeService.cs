using Lagrange.Core.Common;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<SendLikeEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x7e5_104")]
internal sealed class SendLikeService : OidbService<SendLikeEventReq, SendLikeEventResp, D7E5ReqBody, D7E5RespBody>
{
    protected override uint Command => 0x7e5;
    protected override uint Service => 104;

    protected override async Task<D7E5ReqBody> ProcessRequest(SendLikeEventReq request, BotContext context)
    {
        var stranger = await context.CacheContext.ResolveStranger(request.UserUin);
        return new D7E5ReqBody
        {
            TargetUid = stranger.Uid,
            SourceId = 71,
            Count = request.Count,
        };
    }

    protected override Task<SendLikeEventResp> ProcessResponse(D7E5RespBody response, BotContext context) =>
        Task.FromResult(SendLikeEventResp.Default);
}
