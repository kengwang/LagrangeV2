using Lagrange.Core.Common;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;
namespace Lagrange.Core.Internal.Services.System;
[EventSubscribe<FetchGroupRobotOptionEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0xef0_1")]
internal sealed class FetchGroupRobotOptionService : OidbService<FetchGroupRobotOptionEventReq, FetchGroupRobotOptionEventResp, EF0ReqBody, EF0RspBody>
{
    protected override uint Command => 0xef0;
    protected override uint Service => 1;
    protected override Task<EF0ReqBody> ProcessRequest(FetchGroupRobotOptionEventReq request, BotContext context) => Task.FromResult(new EF0ReqBody { GroupCodes = [(ulong)request.GroupUin], Filter = new EF0Filter { InviteRobotMemberSwitch = 1, InviteRobotMemberExamine = 1 } });
    protected override Task<FetchGroupRobotOptionEventResp> ProcessResponse(EF0RspBody response, BotContext context)
    {
        var item = response.Items?.FirstOrDefault() ?? throw new OperationException(-1, "Group robot option response is empty.");
        if (item.ResultCode != 0) throw new OperationException((int)item.ResultCode, "Group robot option query failed.");
        return Task.FromResult(new FetchGroupRobotOptionEventResp(item.Ext?.InviteRobotMemberSwitch ?? 0, item.Ext?.InviteRobotMemberExamine ?? 0));
    }
}
