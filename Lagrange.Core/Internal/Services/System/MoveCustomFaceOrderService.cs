using Lagrange.Core.Common;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<MoveCustomFaceOrderEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x902e_1")]
internal sealed class MoveCustomFaceOrderService : OidbService<MoveCustomFaceOrderEventReq, MoveCustomFaceOrderEventResp, D902EBody, D902EResponse>
{
    protected override uint Command => 0x902e;
    protected override uint Service => 1;
    protected override uint Reserved => 1;

    protected override Task<D902EBody> ProcessRequest(MoveCustomFaceOrderEventReq request, BotContext context) => Task.FromResult(new D902EBody
    {
        Field1 = 1,
        OsVersion = "10.0.26200",
        OpType = 2,
        Emojis = [.. request.Entries.Select(entry => new D902EEmoji { EmojiId = entry.FaceId, Md5 = entry.Md5 })],
    });

    protected override Task<MoveCustomFaceOrderEventResp> ProcessResponse(D902EResponse response, BotContext context)
    {
        if (response.RetCode != 0) throw new OperationException((int)response.RetCode, response.ErrorMessage);
        return Task.FromResult(MoveCustomFaceOrderEventResp.Default);
    }
}
