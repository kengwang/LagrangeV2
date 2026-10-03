using Lagrange.Core.Common;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<SetGroupAddOptionEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x89a_0")]
internal sealed class SetGroupAddOptionService : OidbService<SetGroupAddOptionEventReq, SetGroupAddOptionEventResp, D89AReqBody, D89ARspBody>
{
    protected override uint Command => 0x89a;
    protected override uint Service => 0;
    protected override Task<D89AReqBody> ProcessRequest(SetGroupAddOptionEventReq request, BotContext context) => Task.FromResult(new D89AReqBody { GroupCode = request.GroupUin, Group = new() { AddOption = request.AddType, GroupQuestion = request.Question, GroupAnswer = request.AddType == 4 ? request.Answer : string.Empty } });
    protected override Task<SetGroupAddOptionEventResp> ProcessResponse(D89ARspBody response, BotContext context) => Task.FromResult(SetGroupAddOptionEventResp.Default);
}

[EventSubscribe<SetGroupSearchEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x89a_0")]
internal sealed class SetGroupSearchService : OidbService<SetGroupSearchEventReq, SetGroupSearchEventResp, D89AReqBody, D89ARspBody>
{
    protected override uint Command => 0x89a;
    protected override uint Service => 0;
    protected override Task<D89AReqBody> ProcessRequest(SetGroupSearchEventReq request, BotContext context) => Task.FromResult(new D89AReqBody { GroupCode = request.GroupUin, Group = new() { NoFingerOpenFlag = request.NoFingerOpen, NoCodeFingerOpenFlag = request.NoCodeFingerOpen } });
    protected override Task<SetGroupSearchEventResp> ProcessResponse(D89ARspBody response, BotContext context) => Task.FromResult(SetGroupSearchEventResp.Default);
}

[EventSubscribe<SetGroupNewMemberHistoryEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x89a_0")]
internal sealed class SetGroupNewMemberHistoryService : OidbService<SetGroupNewMemberHistoryEventReq, SetGroupNewMemberHistoryEventResp, D89AReqBody, D89ARspBody>
{
    protected override uint Command => 0x89a;
    protected override uint Service => 0;
    protected override Task<D89AReqBody> ProcessRequest(SetGroupNewMemberHistoryEventReq request, BotContext context) => Task.FromResult(new D89AReqBody { GroupCode = request.GroupUin, Group = new() { GroupFlagExt4 = request.Visible ? 4u : 0u, GroupFlagExt4Mask = 4u } });
    protected override Task<SetGroupNewMemberHistoryEventResp> ProcessResponse(D89ARspBody response, BotContext context) => Task.FromResult(SetGroupNewMemberHistoryEventResp.Default);
}

[EventSubscribe<SetGroupInvitePolicyEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x89a_0")]
internal sealed class SetGroupInvitePolicyService : OidbService<SetGroupInvitePolicyEventReq, SetGroupInvitePolicyEventResp, D89AReqBody, D89ARspBody>
{
    protected override uint Command => 0x89a;
    protected override uint Service => 0;
    protected override Task<D89AReqBody> ProcessRequest(SetGroupInvitePolicyEventReq request, BotContext context)
    {
        const uint mask = 0x06100000;
        var bits = request.Policy switch { "disabled" => 0x04000000u, "no_approval" => 0x00100000u, "no_approval_under_100" => 0x02000000u, "require_approval" => 0u, _ => throw new ArgumentException("Unknown invite policy.", nameof(request)) };
        return Task.FromResult(new D89AReqBody { GroupCode = request.GroupUin, Group = new() { AppPrivilegeFlag = (request.PrivilegeFlag & ~mask) | bits, AppPrivilegeMask = mask, AllowMemberInvite = request.Policy == "disabled" ? 0u : 1u } });
    }
    protected override Task<SetGroupInvitePolicyEventResp> ProcessResponse(D89ARspBody response, BotContext context) => Task.FromResult(SetGroupInvitePolicyEventResp.Default);
}
