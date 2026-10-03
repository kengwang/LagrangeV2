using Lagrange.Core.Common;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<FetchDoubtFriendRequestsEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0xd69_0")]
internal sealed class FetchDoubtFriendRequestsService : OidbService<FetchDoubtFriendRequestsEventReq, FetchDoubtFriendRequestsEventResp, D69GetReq, D69GetResp>
{
    protected override uint Command => 0xd69;
    protected override uint Service => 0;

    protected override Task<D69GetReq> ProcessRequest(FetchDoubtFriendRequestsEventReq request, BotContext context) =>
        Task.FromResult(new D69GetReq { Operation = 1, Inner = new D69GetReqInner { Count = 100, Cookie = string.Empty } });

    protected override Task<FetchDoubtFriendRequestsEventResp> ProcessResponse(D69GetResp response, BotContext context)
    {
        if (response.Status != 1) throw new OperationException((int)response.Status, response.Body?.Reason ?? "Failed to fetch doubtful friend requests.");
        var result = (response.Body?.Items ?? []).Select(item => new BotDoubtFriendRequest
        {
            Uid = item.Uid ?? string.Empty, UserId = checked((long)item.Uin), Nick = item.Nick ?? string.Empty,
            Source = item.Source ?? string.Empty, Reason = item.Reason ?? string.Empty, Message = item.Message ?? string.Empty,
            GroupCode = item.GroupCode ?? string.Empty, RequestTime = checked((long)item.RequestTime)
        }).ToList();
        return Task.FromResult(new FetchDoubtFriendRequestsEventResp(result));
    }
}

[EventSubscribe<HandleDoubtApprovalEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0xd69_0")]
internal sealed class HandleDoubtApprovalService : OidbService<HandleDoubtApprovalEventReq, HandleDoubtApprovalEventResp, D69ApproveReq, DEmptyResp>
{
    protected override uint Command => 0xd69;
    protected override uint Service => 0;
    protected override Task<D69ApproveReq> ProcessRequest(HandleDoubtApprovalEventReq request, BotContext context) => Task.FromResult(new D69ApproveReq { Uid = request.Uid, TargetUid = request.Uid });
    protected override Task<HandleDoubtApprovalEventResp> ProcessResponse(DEmptyResp response, BotContext context) => Task.FromResult(HandleDoubtApprovalEventResp.Instance);
}

[EventSubscribe<HandleDoubtDeleteEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0xd69_0")]
internal sealed class HandleDoubtDeleteService : OidbService<HandleDoubtDeleteEventReq, HandleDoubtDeleteEventResp, D69DeleteReq, DEmptyResp>
{
    protected override uint Command => 0xd69;
    protected override uint Service => 0;
    protected override Task<D69DeleteReq> ProcessRequest(HandleDoubtDeleteEventReq request, BotContext context) => Task.FromResult(new D69DeleteReq { Operation = 3, Inner = new D69DeleteReqInner { Uid = request.Uid } });
    protected override Task<HandleDoubtDeleteEventResp> ProcessResponse(DEmptyResp response, BotContext context) => Task.FromResult(HandleDoubtDeleteEventResp.Instance);
}
