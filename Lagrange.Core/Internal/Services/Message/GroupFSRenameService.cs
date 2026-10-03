using Lagrange.Core.Common;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.Message;

[EventSubscribe<GroupFSRenameEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x6d6_4")]
internal sealed class GroupFSRenameService : OidbService<GroupFSRenameEventReq, GroupFSRenameEventResp, D6D6ReqBody, D6D6RspBody>
{
    protected override uint Command => 0x6d6;
    protected override uint Service => 4;

    protected override Task<D6D6ReqBody> ProcessRequest(GroupFSRenameEventReq request, BotContext context) => Task.FromResult(new D6D6ReqBody
    {
        RenameFileReq = new RenameFileReqBody
        {
            Uint64GroupCode = checked((ulong)request.GroupUin),
            Uint32AppId = 7,
            Uint32BusId = 102,
            StrFileId = request.FileId,
            StrParentFolderId = request.ParentDirectory,
            StrNewFileName = request.NewFileName,
        },
    });

    protected override Task<GroupFSRenameEventResp> ProcessResponse(D6D6RspBody response, BotContext context)
    {
        var result = response.RenameFileRsp;
        if (result.Int32RetCode != 0) throw new OperationException((int)result.Int32RetCode, result.StrRetMsg);
        return Task.FromResult(new GroupFSRenameEventResp());
    }
}
