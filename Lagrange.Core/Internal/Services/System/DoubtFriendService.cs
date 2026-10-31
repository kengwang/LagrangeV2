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

    protected override async Task<FetchDoubtFriendRequestsEventResp> ProcessResponse(D69GetResp response, BotContext context)
    {
        if (response.Status != 1) throw new OperationException((int)response.Status, response.Body?.Reason ?? "Failed to fetch doubtful friend requests.");
        var result = new List<BotDoubtFriendRequest>();
        foreach (var item in response.Body?.Items ?? [])
        {
            long uin;
            string uid;
            if (long.TryParse(item.Uid, out var account) && account > 0)
            {
                var contact = await context.CacheContext.ResolveStranger(account);
                uin = account; uid = contact.Uid;
                if (string.IsNullOrWhiteSpace(uid)) throw new InvalidDataException("Applicant UID is unavailable.");
            }
            else
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(item.Uid);
                var contact = await context.CacheContext.ResolveStranger(item.Uid);
                uin = contact.Uin; uid = item.Uid;
            }
            result.Add(new BotDoubtFriendRequest
            {
            Uid = uid, UserId = uin, Nick = item.Nick ?? string.Empty,
            Source = item.Source ?? string.Empty, Reason = item.Reason ?? string.Empty, Message = item.Message ?? string.Empty,
            GroupCode = item.GroupCode.ToString(global::System.Globalization.CultureInfo.InvariantCulture), RequestTime = checked((long)item.RequestTime)
            });
        }
        return new FetchDoubtFriendRequestsEventResp(result);
    }
}

[EventSubscribe<HandleDoubtApprovalEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0xd72_0")]
internal sealed class HandleDoubtApprovalService : OidbService<HandleDoubtApprovalEventReq, HandleDoubtApprovalEventResp, D69ApproveReq, DEmptyResp>
{
    protected override uint Command => 0xd72;
    protected override uint Service => 0;
    protected override Task<D69ApproveReq> ProcessRequest(HandleDoubtApprovalEventReq request, BotContext context)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(context.Keystore.Uid);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Uid);
        return Task.FromResult(new D69ApproveReq { Uid = context.Keystore.Uid, TargetUid = request.Uid });
    }
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
