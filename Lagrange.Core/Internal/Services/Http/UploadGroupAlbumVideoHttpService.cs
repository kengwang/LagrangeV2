using Lagrange.Core.Common;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Http;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.Http;

[HttpService("group.upload_album_video", "POST", "/webapp/json/sliceUpload", "qzone.qq.com", AuthInjection = HttpAuthInjection.UrlBknFromPSkey, BodyPSkeyPath = "control_req.0.token.data")]
[EventSubscribe<UploadGroupAlbumVideoEventReq>(Protocols.All)]
internal sealed class UploadGroupAlbumVideoHttpService : HttpService<UploadGroupAlbumVideoEventReq, UploadGroupAlbumVideoEventResp>
{
    public override async Task<UploadGroupAlbumVideoEventResp> ExecuteAsync(BotContext context, UploadGroupAlbumVideoEventReq request, CancellationToken cancellationToken = default)
    {
        var uploader = new GroupAlbumVideoUploader(async (uri, content, ct) =>
        {
            using var message = CreateRequest(HttpMethod.Post, uri, content);
            var payload = await SendRequestAsync(context, message, ct).ConfigureAwait(false);
            return HttpResponseParser.ParseJson(payload, "group.upload_album_video");
        });
        return new(await uploader.Upload(context.BotUin, request, cancellationToken).ConfigureAwait(false));
    }

    protected override Task<HttpRequestMessage> BuildRequestAsync(BotContext context, UploadGroupAlbumVideoEventReq request, CancellationToken cancellationToken) =>
        throw new InvalidOperationException("Video upload uses video and cover control requests followed by slices.");
    protected override Task<UploadGroupAlbumVideoEventResp> ParseResponseAsync(BotContext context, UploadGroupAlbumVideoEventReq request, HttpResponseMessage response, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken) =>
        throw new InvalidOperationException("Video upload parses each control and slice response individually.");
}
