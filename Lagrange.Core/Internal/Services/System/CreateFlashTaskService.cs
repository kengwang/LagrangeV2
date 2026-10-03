using Lagrange.Core.Common;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<CreateFlashTaskEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x93cf_1")]
internal sealed class CreateFlashTaskService : OidbService<CreateFlashTaskEventReq, CreateFlashTaskEventResp, D93CFReq, D93CFResp>
{
    protected override uint Command => 0x93cf;
    protected override uint Service => 1;
    protected override Task<D93CFReq> ProcessRequest(CreateFlashTaskEventReq request, BotContext context) => Task.FromResult(new D93CFReq
    {
        Field1 = 1, TypeCode = request.FileType, Field12 = 1,
        FileInfo = new D93CFInfo { FileName = request.FileName, OriginalName = request.FileName, FileType = 1, FileSize = request.FileSize, Uploader = new D93CFUploader { Uin = context.Keystore.Uin.ToString(), Uid = context.Keystore.Uid, Nickname = context.BotInfo?.Name ?? string.Empty } }
    });
    protected override Task<CreateFlashTaskEventResp> ProcessResponse(D93CFResp response, BotContext context)
    {
        if (string.IsNullOrWhiteSpace(response.FilesetUuid) || string.IsNullOrWhiteSpace(response.UploadKey)) throw new Lagrange.Core.Exceptions.OperationException(-1, "Flash fileset creation returned an empty identifier.");
        return Task.FromResult(new CreateFlashTaskEventResp(new BotFlashCreateResult { FilesetUuid = response.FilesetUuid!, UploadKey = response.UploadKey!, UploadUrl = response.UploadUrl, Expire = response.Expire, Ttl = response.Ttl }));
    }
}
