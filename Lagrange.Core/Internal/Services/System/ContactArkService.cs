using Lagrange.Core.Common;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<FetchBuddyArkEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x12b6_0")]
internal sealed class FetchBuddyArkService : OidbService<FetchBuddyArkEventReq, FetchBuddyArkEventResp, D12B6Req, D12B6Resp>
{
    protected override uint Command => 0x12b6;
    protected override uint Service => 0;
    protected override Task<D12B6Req> ProcessRequest(FetchBuddyArkEventReq request, BotContext context)
    {
        if (request.UserUin <= 0) throw new InvalidTargetException(request.UserUin);
        return Task.FromResult(new D12B6Req { Uin = checked((ulong)request.UserUin), Phone = string.Empty, JumpUrl = string.Empty });
    }
    protected override Task<FetchBuddyArkEventResp> ProcessResponse(D12B6Resp response, BotContext context)
    {
        if (string.IsNullOrWhiteSpace(response.Ark)) throw new OperationException(-1, "Buddy Ark response is empty.");
        return Task.FromResult(new FetchBuddyArkEventResp(response.Ark));
    }
}

[EventSubscribe<FetchGroupArkEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x8b7_5")]
internal sealed class FetchGroupArkService : OidbService<FetchGroupArkEventReq, FetchGroupArkEventResp, D8B7Req, D8B7Resp>
{
    protected override uint Command => 0x8b7;
    protected override uint Service => 5;
    protected override Task<D8B7Req> ProcessRequest(FetchGroupArkEventReq request, BotContext context)
    {
        if (request.GroupUin <= 0) throw new InvalidTargetException(request.GroupUin);
        return Task.FromResult(new D8B7Req { RequestType = 1, GroupCode = checked((ulong)request.GroupUin), Flag = 1 });
    }
    protected override Task<FetchGroupArkEventResp> ProcessResponse(D8B7Resp response, BotContext context)
    {
        if (response.ErrorCode != 0) throw new OperationException((int)response.ErrorCode, "Group Ark request failed.");
        if (string.IsNullOrWhiteSpace(response.ArkJson)) throw new OperationException(-1, "Group Ark response is empty.");
        return Task.FromResult(new FetchGroupArkEventResp(response.ArkJson));
    }
}
