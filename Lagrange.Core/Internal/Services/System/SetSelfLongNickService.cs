using Lagrange.Core.Common;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<SetSelfLongNickEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x112a_2")]
internal sealed class SetSelfLongNickService : OidbService<SetSelfLongNickEventReq, SetSelfLongNickEventResp, D112ASingleReqBody, D112ARespBody>
{
    protected override uint Command => 0x112a;
    protected override uint Service => 2;

    protected override Task<D112ASingleReqBody> ProcessRequest(SetSelfLongNickEventReq request, BotContext context) =>
        Task.FromResult(new D112ASingleReqBody
        {
            Uin = checked((ulong)context.BotUin),
            Profile = new D112AProfileString { FieldId = 102, Value = request.LongNick },
        });

    protected override Task<SetSelfLongNickEventResp> ProcessResponse(D112ARespBody response, BotContext context) =>
        Task.FromResult(SetSelfLongNickEventResp.Default);
}
