using Lagrange.Core.Common;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<SetFriendRemarkEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x912e_0")]
internal sealed class SetFriendRemarkService : OidbService<SetFriendRemarkEventReq, SetFriendRemarkEventResp, D912EReqBody, D912ERspBody>
{
    protected override uint Command => 0x912e;
    protected override uint Service => 0;

    protected override async Task<D912EReqBody> ProcessRequest(SetFriendRemarkEventReq request, BotContext context)
    {
        var friend = await context.CacheContext.ResolveFriend(request.UserUin)
            ?? throw new InvalidTargetException(request.UserUin);

        return new D912EReqBody
        {
            Change = new D912EChange
            {
                Target = new D912ETarget { TargetUid = friend.Uid },
                Remark = request.Remark,
            },
            Scene = 0,
        };
    }

    protected override Task<SetFriendRemarkEventResp> ProcessResponse(D912ERspBody response, BotContext context)
    {
        if (response.Error is { Code: not 0 } error)
            throw new OperationException(error.Code, error.Message);

        return Task.FromResult(SetFriendRemarkEventResp.Default);
    }
}
