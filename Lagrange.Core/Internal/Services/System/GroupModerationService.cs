using Lagrange.Core.Common;
using Lagrange.Core.Internal.Events;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<GroupSetAdminEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x1096_1")]
internal class GroupSetAdminService : OidbService<GroupSetAdminEventReq, GroupSetAdminEventResp, GroupSetAdminRequest, GroupSetAdminResponse>
{
    private protected override uint Command => 0x1096;

    private protected override uint Service => 1;

    private protected override Task<GroupSetAdminRequest> ProcessRequest(GroupSetAdminEventReq request, BotContext context)
    {
        return Task.FromResult(new GroupSetAdminRequest
        {
            GroupUin = request.GroupUin,
            Uid = request.Uid,
            IsAdmin = request.IsAdmin
        });
    }

    private protected override Task<GroupSetAdminEventResp> ProcessResponse(GroupSetAdminResponse response, BotContext context)
    {
        return Task.FromResult(GroupSetAdminEventResp.Default);
    }
}

[EventSubscribe<GroupMuteMemberEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x1253_1")]
internal class GroupMuteMemberService : OidbService<GroupMuteMemberEventReq, GroupMuteMemberEventResp, GroupMuteMemberRequest, GroupMuteMemberResponse>
{
    private protected override uint Command => 0x1253;

    private protected override uint Service => 1;

    private protected override Task<GroupMuteMemberRequest> ProcessRequest(GroupMuteMemberEventReq request, BotContext context)
    {
        return Task.FromResult(new GroupMuteMemberRequest
        {
            GroupUin = request.GroupUin,
            Type = 1,
            Body = new GroupMuteMemberRequestBody
            {
                TargetUid = request.Uid,
                Duration = request.Duration
            }
        });
    }

    private protected override Task<GroupMuteMemberEventResp> ProcessResponse(GroupMuteMemberResponse response, BotContext context)
    {
        return Task.FromResult(GroupMuteMemberEventResp.Default);
    }
}

[EventSubscribe<GroupMuteGlobalEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x89a_0")]
internal class GroupMuteGlobalService : OidbService<GroupMuteGlobalEventReq, GroupMuteGlobalEventResp, GroupMuteGlobalRequest, GroupMuteGlobalResponse>
{
    private protected override uint Command => 0x89a;

    private protected override uint Service => 0;

    private protected override Task<GroupMuteGlobalRequest> ProcessRequest(GroupMuteGlobalEventReq request, BotContext context)
    {
        return Task.FromResult(new GroupMuteGlobalRequest
        {
            GroupUin = request.GroupUin,
            State = new GroupMuteGlobalState { S = request.IsMute ? uint.MaxValue : 0u }
        });
    }

    private protected override Task<GroupMuteGlobalEventResp> ProcessResponse(GroupMuteGlobalResponse response, BotContext context)
    {
        return Task.FromResult(GroupMuteGlobalEventResp.Default);
    }
}

[EventSubscribe<GroupKickMemberEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x8a0_1")]
internal class GroupKickMemberService : OidbService<GroupKickMemberEventReq, GroupKickMemberEventResp, GroupKickMemberRequest, GroupKickMemberResponse>
{
    private protected override uint Command => 0x8a0;

    private protected override uint Service => 1;

    private protected override Task<GroupKickMemberRequest> ProcessRequest(GroupKickMemberEventReq request, BotContext context)
    {
        return Task.FromResult(new GroupKickMemberRequest
        {
            GroupUin = request.GroupUin,
            TargetUid = request.Uid,
            RejectAddRequest = request.RejectAddRequest
        });
    }

    private protected override Task<GroupKickMemberEventResp> ProcessResponse(GroupKickMemberResponse response, BotContext context)
    {
        return Task.FromResult(GroupKickMemberEventResp.Default);
    }
}
