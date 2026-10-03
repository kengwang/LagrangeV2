using Lagrange.Core.Common;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.Message;

[EventSubscribe<GroupFSTransferEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x6d9_0")]
internal sealed class GroupFSTransferService : OidbService<GroupFSTransferEventReq, GroupFSTransferEventResp, D6D9ReqBody, D6D9RspBody>
{
    protected override uint Command => 0x6d9;
    protected override uint Service => 0;

    protected override Task<D6D9ReqBody> ProcessRequest(GroupFSTransferEventReq request, BotContext context) => Task.FromResult(new D6D9ReqBody
    {
        TransFileReq = new TransFileReqBody
        {
            GroupCode = checked((ulong)request.GroupUin),
            AppId = 7,
            BusId = 102,
            FileId = request.FileId,
        },
    });

    protected override Task<GroupFSTransferEventResp> ProcessResponse(D6D9RspBody response, BotContext context)
    {
        var result = response.TransFileRsp;
        if (result.RetCode != 0) throw new OperationException((int)result.RetCode, result.RetMsg);
        return Task.FromResult(new GroupFSTransferEventResp((int)result.SaveBusId, result.SaveFilePath));
    }
}
