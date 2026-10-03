using Lagrange.Core.Common;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;
namespace Lagrange.Core.Internal.Services.System;
[EventSubscribe<FetchGroupAdminSettingsEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x88d_110")]
internal sealed class FetchGroupAdminSettingsService : OidbService<FetchGroupAdminSettingsEventReq, FetchGroupAdminSettingsEventResp, D88DReqBody, D88DRspBody>
{
    protected override uint Command => 0x88d;
    protected override uint Service => 110;
    protected override Task<D88DReqBody> ProcessRequest(FetchGroupAdminSettingsEventReq request, BotContext context) => Task.FromResult(new D88DReqBody { AppId = (uint)Random.Shared.Next(), Groups = [new D88DReqGroupInfo { GroupCode = (ulong)request.GroupUin, GroupInfo = new D88DGroupInfo() }] });
    protected override Task<FetchGroupAdminSettingsEventResp> ProcessResponse(D88DRspBody response, BotContext context)
    {
        var item = response.Groups?.FirstOrDefault() ?? throw new OperationException(-1, response.ErrorInfo ?? "Group settings were not returned");
        if (item.Result is not null and not 0) throw new OperationException((int)item.Result.Value, response.ErrorInfo);
        var x = item.GroupInfo ?? throw new OperationException(-1, response.ErrorInfo ?? "Group settings were not returned");
        var historyFlag = x.GroupFlagExt4 ?? 0;
        var privilege = x.AppPrivilegeFlag ?? 0;
        var policy = (privilege & 0x04000000) != 0 ? "disabled" : (privilege & 0x02000000) != 0 ? "no_approval_under_100" : (privilege & 0x00100000) != 0 ? "no_approval" : "require_approval";
        return Task.FromResult(new FetchGroupAdminSettingsEventResp(new BotGroupAdminSettingsResult { AddType = x.GroupOption ?? 0, GroupQuestion = x.GroupQuestion ?? string.Empty, GroupAnswer = x.GroupAnswer ?? string.Empty, MemberInvitePolicy = policy, NewMemberHistoryVisible = historyFlag == 1 || (historyFlag & 4) != 0, NoFingerOpen = x.NoFingerOpenFlag ?? 0, NoCodeFingerOpen = x.NoCodeFingerOpenFlag ?? 0, PrivilegeFlag = privilege }));
    }
}
