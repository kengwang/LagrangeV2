using Lagrange.Core.Common;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<CommitFlashFileEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x93d0_1")]
internal sealed class CommitFlashFileService : OidbService<CommitFlashFileEventReq, CommitFlashFileEventResp, D93D0Req, D93D0Resp>
{
    protected override uint Command => 0x93d0;
    protected override uint Service => 1;
    protected override Task<D93D0Req> ProcessRequest(CommitFlashFileEventReq request, BotContext context) => Task.FromResult(new D93D0Req
    {
        Field1 = 1, FilesetUuid = request.FilesetUuid, UploadKey = request.UploadKey, Field5 = 1, Field6 = 1,
        CommitInfo = [new D93D0Info { FilesetUuid = request.FilesetUuid, FileUuid = request.FileUuid, Index = request.Index, FormatCode = request.FormatCode, FileName = request.FileName, OriginalName = request.FileName, FileSize = request.FileSize }]
    });
    protected override Task<CommitFlashFileEventResp> ProcessResponse(D93D0Resp response, BotContext context)
    {
        if (response.Field1 != 1 || string.IsNullOrWhiteSpace(response.FilesetUuid) || string.IsNullOrWhiteSpace(response.UploadKey)) throw new Lagrange.Core.Exceptions.OperationException(-1, "Flash file commit acknowledgement is missing or invalid.");
        return Task.FromResult(new CommitFlashFileEventResp());
    }
}
