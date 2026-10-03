using Lagrange.Core.Common;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<SetGroupAdminEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x1096_1")]
internal sealed class SetGroupAdminService : OidbService<SetGroupAdminEventReq, SetGroupAdminEventResp, D1096ReqBody, D1096RespBody>
{
    protected override uint Command => 0x1096;
    protected override uint Service => 1;

    protected override async Task<D1096ReqBody> ProcessRequest(SetGroupAdminEventReq request, BotContext context)
    {
        var uid = context.CacheContext.ResolveCachedUid(request.UserUin);
        if (uid == null)
        {
            await context.CacheContext.GetMemberList(request.GroupUin, true);
            uid = context.CacheContext.ResolveCachedUid(request.UserUin);
        }

        return new D1096ReqBody
        {
            GroupUin = checked((ulong)request.GroupUin),
            Uid = uid ?? throw new InvalidTargetException(request.UserUin),
            IsAdmin = request.Enable,
        };
    }

    protected override Task<SetGroupAdminEventResp> ProcessResponse(D1096RespBody response, BotContext context) =>
        Task.FromResult(SetGroupAdminEventResp.Default);
}
