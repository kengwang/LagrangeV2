using Lagrange.Core.Common;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<SetGroupMemberPermissionEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x89a_0")]
internal sealed class SetGroupMemberPermissionService : OidbService<SetGroupMemberPermissionEventReq, SetGroupMemberPermissionEventResp, D89AReqBody, D89ARspBody>
{
    protected override uint Command => 0x89a;
    protected override uint Service => 0;

    protected override Task<D89AReqBody> ProcessRequest(SetGroupMemberPermissionEventReq request, BotContext context) =>
        Task.FromResult(new D89AReqBody
        {
            GroupCode = request.GroupUin,
            Group = new D89AReqBody.GroupInfo { AppPrivilegeFlag = request.PrivilegeFlag, AppPrivilegeMask = request.PrivilegeMask },
        });

    protected override Task<SetGroupMemberPermissionEventResp> ProcessResponse(D89ARspBody response, BotContext context)
    {
        if (!string.IsNullOrWhiteSpace(response.ErrorInfo)) throw new OperationException(-1, response.ErrorInfo);
        return Task.FromResult(new SetGroupMemberPermissionEventResp());
    }
}
