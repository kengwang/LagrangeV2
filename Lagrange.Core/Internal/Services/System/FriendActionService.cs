using Lagrange.Core.Common;
using Lagrange.Core.Internal.Events;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<FriendLikeEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x7e5_104")]
internal class FriendLikeService : OidbService<FriendLikeEventReq, FriendLikeEventResp, FriendLikeRequest, FriendLikeResponse>
{
    private protected override uint Command => 0x7e5;

    private protected override uint Service => 104;

    private protected override Task<FriendLikeRequest> ProcessRequest(FriendLikeEventReq request, BotContext context)
    {
        return Task.FromResult(new FriendLikeRequest
        {
            TargetUid = request.TargetUid,
            Field2 = 71,
            Count = request.Count
        });
    }

    private protected override Task<FriendLikeEventResp> ProcessResponse(FriendLikeResponse response, BotContext context)
    {
        return Task.FromResult(FriendLikeEventResp.Default);
    }
}

[EventSubscribe<DeleteFriendEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x126b_0")]
internal class DeleteFriendService : OidbService<DeleteFriendEventReq, DeleteFriendEventResp, DeleteFriendRequest, DeleteFriendResponse>
{
    private protected override uint Command => 0x126b;

    private protected override uint Service => 0;

    private protected override Task<DeleteFriendRequest> ProcessRequest(DeleteFriendEventReq request, BotContext context)
    {
        return Task.FromResult(new DeleteFriendRequest
        {
            Body = new DeleteFriendRequestBody
            {
                TargetUid = request.TargetUid,
                Block = false
            }
        });
    }

    private protected override Task<DeleteFriendEventResp> ProcessResponse(DeleteFriendResponse response, BotContext context)
    {
        return Task.FromResult(DeleteFriendEventResp.Default);
    }
}
