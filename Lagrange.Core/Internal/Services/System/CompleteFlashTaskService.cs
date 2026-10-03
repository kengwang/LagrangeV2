using Lagrange.Core.Common;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<CompleteFlashTaskEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x93db_1")]
internal sealed class CompleteFlashTaskService : OidbService<CompleteFlashTaskEventReq, CompleteFlashTaskEventResp, D93DBReq, DEmptyResp>
{
    protected override uint Command => 0x93db;
    protected override uint Service => 1;
    protected override Task<D93DBReq> ProcessRequest(CompleteFlashTaskEventReq request, BotContext context) => Task.FromResult(new D93DBReq { FilesetUuid = request.FilesetUuid });
    protected override Task<CompleteFlashTaskEventResp> ProcessResponse(DEmptyResp response, BotContext context) => Task.FromResult(new CompleteFlashTaskEventResp());
}
