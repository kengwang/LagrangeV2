using Lagrange.Core.Common;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<SetFriendCategoryEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x1255_0")]
internal sealed class SetFriendCategoryService : OidbService<SetFriendCategoryEventReq, SetFriendCategoryEventResp, D1255ReqBody, D1255RespBody>
{
    protected override uint Command => 0x1255;
    protected override uint Service => 0;

    protected override async Task<D1255ReqBody> ProcessRequest(SetFriendCategoryEventReq request, BotContext context)
    {
        var friend = await context.CacheContext.ResolveFriend(request.UserUin)
            ?? throw new InvalidTargetException(request.UserUin);
        return new D1255ReqBody { Uid = friend.Uid, CategoryId = checked((uint)request.CategoryId) };
    }

    protected override Task<SetFriendCategoryEventResp> ProcessResponse(D1255RespBody response, BotContext context) =>
        Task.FromResult(SetFriendCategoryEventResp.Default);
}
