using Lagrange.Core.Common;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Internal.Events;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<FetchFriendRequestsEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x5cf_11")]
internal class FetchFriendRequestsService : OidbService<FetchFriendRequestsEventReq, FetchFriendRequestsEventResp, FetchFriendRequestsRequest, FetchFriendRequestsResponse>
{
    private protected override uint Command => 0x5cf;

    private protected override uint Service => 11;

    private protected override Task<FetchFriendRequestsRequest> ProcessRequest(FetchFriendRequestsEventReq request, BotContext context)
    {
        return Task.FromResult(new FetchFriendRequestsRequest
        {
            Field1 = 1,
            Field3 = 6,
            SelfUid = context.Keystore.Uid ?? string.Empty,
            Field5 = 0,
            Field6 = 80,
            Field8 = 2,
            Field9 = 0,
            Field12 = 1,
            Field22 = 1
        });
    }

    private protected override Task<FetchFriendRequestsEventResp> ProcessResponse(FetchFriendRequestsResponse response, BotContext context)
    {
        var requests = response.Info.Requests
            .Select(request => new BotFriendRequest(request.TargetUid, request.SourceUid, request.State, request.Comment, request.Source, request.Timestamp))
            .ToList();

        return Task.FromResult(new FetchFriendRequestsEventResp(requests));
    }
}

[EventSubscribe<SetFriendRequestEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0xb5d_44")]
internal class SetFriendRequestService : OidbService<SetFriendRequestEventReq, SetFriendRequestEventResp, SetFriendRequestRequest, SetFriendRequestResponse>
{
    private protected override uint Command => 0xb5d;

    private protected override uint Service => 44;

    private protected override Task<SetFriendRequestRequest> ProcessRequest(SetFriendRequestEventReq request, BotContext context)
    {
        return Task.FromResult(new SetFriendRequestRequest
        {
            Accept = request.Accept ? 3u : 5u,
            TargetUid = request.TargetUid
        });
    }

    private protected override Task<SetFriendRequestEventResp> ProcessResponse(SetFriendRequestResponse response, BotContext context)
    {
        return Task.FromResult(SetFriendRequestEventResp.Default);
    }
}
