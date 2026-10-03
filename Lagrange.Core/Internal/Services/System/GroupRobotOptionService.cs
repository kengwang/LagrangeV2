using Lagrange.Core.Common;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;
namespace Lagrange.Core.Internal.Services.System;
[EventSubscribe<SetGroupRobotOptionEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0xf00_3")]
internal sealed class SetGroupRobotOptionService : OidbService<SetGroupRobotOptionEventReq, SetGroupRobotOptionEventResp, F00ReqBody, F00RspBody>
{
    protected override uint Command => 0xf00;
    protected override uint Service => 3;
    protected override Task<F00ReqBody> ProcessRequest(SetGroupRobotOptionEventReq request, BotContext context) => Task.FromResult(new F00ReqBody { GroupCode = request.GroupUin, Info = new F00GroupInfo { GroupCode = request.GroupUin, Ext = new F00ExtInfo { InviteRobotMemberSwitch = request.MemberSwitch, InviteRobotMemberExamine = request.MemberExamine } } });
    protected override Task<SetGroupRobotOptionEventResp> ProcessResponse(F00RspBody response, BotContext context) { if (response.Result != 0) throw new OperationException(response.Result, "Group robot option update failed."); return Task.FromResult(SetGroupRobotOptionEventResp.Default); }
}
