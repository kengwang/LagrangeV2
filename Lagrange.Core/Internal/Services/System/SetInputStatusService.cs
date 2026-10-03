using Lagrange.Core.Common;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<SetInputStatusEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0xcd4_1")]
internal sealed class SetInputStatusService : OidbService<SetInputStatusEventReq, SetInputStatusEventResp, DCD4Req, DCD4RespBody>
{
    protected override uint Command => 0xcd4;
    protected override uint Service => 1;

    protected override async Task<DCD4Req> ProcessRequest(SetInputStatusEventReq request, BotContext context)
    {
        var stranger = await context.CacheContext.ResolveStranger(request.UserUin);
        return new DCD4Req
        {
            ReqBody = new DCD4ReqBody { Uid = stranger.Uid, ChatType = 0, EventType = request.EventType },
        };
    }

    protected override Task<SetInputStatusEventResp> ProcessResponse(DCD4RespBody response, BotContext context) =>
        Task.FromResult(SetInputStatusEventResp.Default);
}
