using Lagrange.Core.Common;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<TranslateEnToZhEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x990_2")]
internal sealed class TranslateEnToZhService : OidbService<TranslateEnToZhEventReq, TranslateEnToZhEventResp, D990ReqBody, D990RespBody>
{
    protected override uint Command => 0x990;
    protected override uint Service => 2;
    protected override Task<D990ReqBody> ProcessRequest(TranslateEnToZhEventReq request, BotContext context) => Task.FromResult(new D990ReqBody { TranslateReq = new D990TranslateReq { SourceLanguage = "en", DestinationLanguage = "zh", Words = [.. request.Words] }, Tag10 = 1, Tag12 = 1 });
    protected override Task<TranslateEnToZhEventResp> ProcessResponse(D990RespBody response, BotContext context) => response.TranslateResp is null ? throw new OperationException(-1, "0x990 returned an empty translation response.") : Task.FromResult(new TranslateEnToZhEventResp(response.TranslateResp.DestinationWords));
}

[EventSubscribe<ImageOcrEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0xe07_0")]
internal sealed class ImageOcrService : OidbService<ImageOcrEventReq, ImageOcrEventResp, DE07ReqBody, DE07RespBody>
{
    protected override uint Command => 0xe07;
    protected override uint Service => 0;
    protected override Task<DE07ReqBody> ProcessRequest(ImageOcrEventReq request, BotContext context) => Task.FromResult(new DE07ReqBody { Version = 1, Client = 0, Entrance = 1, OcrReqBody = new DE07OcrReqBody { ImageUrl = request.ImageUrl } });
    protected override Task<ImageOcrEventResp> ProcessResponse(DE07RespBody response, BotContext context)
    {
        if (response.RetCode != 0) throw new OperationException(response.RetCode, string.IsNullOrWhiteSpace(response.ErrMsg) ? response.Wording : response.ErrMsg);
        var result = new BotOcrResult { Language = response.OcrRspBody?.Language ?? string.Empty, Texts = [.. (response.OcrRspBody?.TextDetections ?? []).Select(x => new BotOcrText { Text = x.DetectedText, Confidence = x.Confidence, Coordinates = [.. (x.Polygon?.Coordinates ?? []).Select(p => new BotOcrCoordinate(p.X, p.Y))] })] };
        return Task.FromResult(new ImageOcrEventResp(result));
    }
}
