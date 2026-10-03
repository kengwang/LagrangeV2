using Lagrange.Core.Common;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<DeleteFriendEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x126b_0")]
internal sealed class DeleteFriendService : OidbService<DeleteFriendEventReq, DeleteFriendEventResp, D126BReqBody, D126BRespBody>
{
    protected override uint Command => 0x126b;
    protected override uint Service => 0;

    protected override async Task<D126BReqBody> ProcessRequest(DeleteFriendEventReq request, BotContext context)
    {
        var friend = await context.CacheContext.ResolveFriend(request.UserUin)
            ?? throw new InvalidTargetException(request.UserUin);

        return new D126BReqBody
        {
            Field1 = new D126BField1
            {
                TargetUid = friend.Uid,
                Field2 = new D126BField2
                {
                    Field1 = 130,
                    Field2 = 109,
                    Field3 = new D126BField3 { Field1 = 8, Field2 = 8, Field3 = 50 },
                },
                Block = request.Block,
                Field4 = true,
            },
        };
    }

    protected override Task<DeleteFriendEventResp> ProcessResponse(D126BRespBody response, BotContext context) =>
        Task.FromResult(DeleteFriendEventResp.Default);
}
