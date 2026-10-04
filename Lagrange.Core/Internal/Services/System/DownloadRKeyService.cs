using Lagrange.Core.Common;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;
namespace Lagrange.Core.Internal.Services.System;
[EventSubscribe<DownloadRKeyEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x9067_202")]
internal sealed class DownloadRKeyService : OidbService<DownloadRKeyEventReq, DownloadRKeyEventResp, NTV2RichMediaReq, NTV2RichMediaResp>
{
    protected override uint Command => 0x9067;
    protected override uint Service => 202;
    protected override uint Reserved => 1;
    protected override Task<NTV2RichMediaReq> ProcessRequest(DownloadRKeyEventReq request, BotContext context) => Task.FromResult(new NTV2RichMediaReq
    {
        ReqHead = new() { Common = new() { RequestId = 1, Command = 202 }, Scene = new() { RequestType = 2, BusinessType = 1, SceneType = 0 }, Client = new() { AgentType = 2 } },
        DownloadRKey = new() { Types = [10, 20, 2] }
    });
    protected override Task<DownloadRKeyEventResp> ProcessResponse(NTV2RichMediaResp response, BotContext context)
    {
        if (response.RespHead?.RetCode is > 0) throw new OperationException((int)response.RespHead.RetCode, response.RespHead.Message);
        return Task.FromResult(new DownloadRKeyEventResp((response.DownloadRKey?.RKeys ?? []).Select(x => new BotDownloadRKey(x.Type, x.Rkey, x.RkeyCreateTime, x.RkeyTtlSec)).ToArray()));
    }
}
