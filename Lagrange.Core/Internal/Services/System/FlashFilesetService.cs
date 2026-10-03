using Lagrange.Core.Common;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.System;

internal static class FlashFilesetMapper
{
    public static BotFlashFilesetResult Map(D93DResp response) => new()
    {
        Entries = [.. (response.Entries ?? []).Where(x => !string.IsNullOrWhiteSpace(x.FilesetUuid)).Select(x => new BotFlashFileEntry
        {
            FilesetUuid = x.FilesetUuid!, FileName = x.FileName ?? string.Empty, OriginalName = x.OriginalName ?? string.Empty,
            FileType = x.FileType, FileSize = x.FileSize, UploadUrl = x.Upload?.Url, FileId = x.File?.FileId, DownloadUrl = x.File?.Download?.Url
        })]
    };
}

[EventSubscribe<ListFlashFilesetsEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x93d2_1")]
internal sealed class ListFlashFilesetsService : OidbService<ListFlashFilesetsEventReq, ListFlashFilesetsEventResp, D93D2Req, D93DResp>
{
    protected override uint Command => 0x93d2;
    protected override uint Service => 1;
    protected override Task<D93D2Req> ProcessRequest(ListFlashFilesetsEventReq request, BotContext context) => Task.FromResult(new D93D2Req { Field1 = 3, Field3 = request.Limit == 0 ? 10 : request.Limit });
    protected override Task<ListFlashFilesetsEventResp> ProcessResponse(D93DResp response, BotContext context) => Task.FromResult(new ListFlashFilesetsEventResp(FlashFilesetMapper.Map(response)));
}

[EventSubscribe<GetFlashFilesetEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x93d3_1")]
internal sealed class GetFlashFilesetService : OidbService<GetFlashFilesetEventReq, GetFlashFilesetEventResp, D93D3Req, D93DResp>
{
    protected override uint Command => 0x93d3;
    protected override uint Service => 1;
    protected override Task<D93D3Req> ProcessRequest(GetFlashFilesetEventReq request, BotContext context) => Task.FromResult(new D93D3Req { FilesetUuid = request.FilesetUuid, Field2 = 7 });
    protected override Task<GetFlashFilesetEventResp> ProcessResponse(D93DResp response, BotContext context) => Task.FromResult(new GetFlashFilesetEventResp(FlashFilesetMapper.Map(response)));
}
