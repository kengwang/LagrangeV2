﻿using Lagrange.Core.Common;
using Lagrange.Core.Internal.Events;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Packets.Service;

namespace Lagrange.Core.Internal.Services.Message;

[EventSubscribe<NudgeEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0xed3_1")]
internal class NudgeService : OidbService<NudgeEventReq, NudgeEventResp, DED3ReqBody, DED3RspBody>
{
    private protected override uint Command => 0xed3;

    private protected override uint Service => 1;
    
    private protected override Task<DED3ReqBody> ProcessRequest(NudgeEventReq request, BotContext context)
    {
        return Task.FromResult(new DED3ReqBody
        {
            ToUin = request.TargetUin,
            GroupCode = request.IsGroup ? request.PeerUin : 0,
            AioUin = !request.IsGroup ? request.PeerUin : 0
        });
    }

    private protected override Task<NudgeEventResp> ProcessResponse(DED3RspBody response, BotContext context)
    {
        return Task.FromResult(new NudgeEventResp());
    }
}