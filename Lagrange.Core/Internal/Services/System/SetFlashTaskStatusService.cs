using Lagrange.Core.Common;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<SetFlashTaskStatusEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x93d1_1")]
internal sealed class SetFlashTaskStatusService : OidbService<SetFlashTaskStatusEventReq, SetFlashTaskStatusEventResp, D93D1Req, DEmptyResp>
{
    protected override uint Command => 0x93d1;
    protected override uint Service => 1;
    protected override Task<D93D1Req> ProcessRequest(SetFlashTaskStatusEventReq request, BotContext context) => Task.FromResult(new D93D1Req { FilesetUuid = request.FilesetUuid, Status = request.Status });
    protected override Task<SetFlashTaskStatusEventResp> ProcessResponse(DEmptyResp response, BotContext context) => Task.FromResult(new SetFlashTaskStatusEventResp());
}
