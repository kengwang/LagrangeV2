using Lagrange.Core.Common;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<ModifyCustomFaceEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x902e_1")]
internal sealed class ModifyCustomFaceService : OidbService<ModifyCustomFaceEventReq, ModifyCustomFaceEventResp, D902EBody, D902EResponse>
{
    protected override uint Command => 0x902e;
    protected override uint Service => 1;
    protected override uint Reserved => 1;

    protected override Task<D902EBody> ProcessRequest(ModifyCustomFaceEventReq request, BotContext context) => Task.FromResult(new D902EBody
    {
        Field1 = 1,
        OsVersion = "10.0.26200",
        OpType = 3,
        Entry = new D902EModifyEntry { Emoji = new D902EEmoji { EmojiId = request.FaceId, Md5 = request.Md5 }, Description = request.Description },
        Field12 = 1,
    });

    protected override Task<ModifyCustomFaceEventResp> ProcessResponse(D902EResponse response, BotContext context)
    {
        if (response.RetCode != 0) throw new OperationException((int)response.RetCode, response.ErrorMessage);
        return Task.FromResult(ModifyCustomFaceEventResp.Default);
    }
}
