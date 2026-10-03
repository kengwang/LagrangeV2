using Lagrange.Core.Common;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Services;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Http;

namespace Lagrange.Core.Internal.Services.Http;

[HttpService("group.upload_album_image", "POST", "/webapp/json/sliceUpload", "qzone.qq.com", AuthInjection = HttpAuthInjection.UrlBknFromSkey, BodyPSkeyPath = "control_req.0.token.data")]
[EventSubscribe<UploadGroupAlbumImageEventReq>(Protocols.All)]
internal sealed class UploadGroupAlbumImageHttpService : HttpService<UploadGroupAlbumImageEventReq, UploadGroupAlbumImageEventResp>
{
    public UploadGroupAlbumImageHttpService() : base("qzone.qq.com") { }

    public override async Task<UploadGroupAlbumImageEventResp> ExecuteAsync(BotContext context, UploadGroupAlbumImageEventReq request, CancellationToken cancellationToken = default)
    {
        var groupUin = request.GroupUin;
        var albumId = request.AlbumId;
        var albumName = request.AlbumName;
        var image = request.Image;
        var fileName = request.FileName;
        if (groupUin <= 0) throw new ArgumentOutOfRangeException(nameof(groupUin));
        if (string.IsNullOrWhiteSpace(albumId)) throw new ArgumentException("Album id is required.", nameof(albumId));
        if (image is null || !image.CanRead) throw new ArgumentException("Image stream is not readable.", nameof(image));
        if (image.CanSeek) image.Position = 0;
        using var buffer = new MemoryStream();
        await image.CopyToAsync(buffer, cancellationToken);
        var bytes = buffer.ToArray();
        if (bytes.Length == 0) throw new ArgumentException("Image cannot be empty.", nameof(image));

        var md5 = Convert.ToHexString(MD5.HashData(bytes)).ToLowerInvariant();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var requestEntry = new JsonObject {
            ["uin"] = context.BotUin.ToString(), ["token"] = new JsonObject { ["type"] = 4, ["data"] = "", ["appid"] = 5 }, ["appid"] = "qun", ["checksum"] = md5,
            ["check_type"] = 0, ["file_len"] = bytes.Length, ["env"] = new JsonObject { ["refer"] = "qzone", ["deviceInfo"] = "h5" }, ["model"] = 0,
            ["biz_req"] = new JsonObject { ["sPicTitle"] = fileName, ["sPicDesc"] = "", ["sAlbumName"] = albumName ?? "", ["sAlbumID"] = albumId, ["iAlbumTypeID"] = 0,
                ["iBitmap"] = 0, ["iUploadType"] = 0, ["iUpPicType"] = 0, ["iBatchID"] = timestamp, ["sPicPath"] = "", ["iPicWidth"] = 0, ["iPicHight"] = 0,
                ["iWaterType"] = 0, ["iDistinctUse"] = 0, ["iNeedFeeds"] = 1, ["iUploadTime"] = timestamp, ["mapExt"] = new JsonObject { ["appid"] = "qun", ["userid"] = groupUin.ToString() },
                ["stExtendInfo"] = new JsonObject { ["mapParams"] = new JsonObject { ["photo_num"] = "1", ["video_num"] = "0", ["batch_num"] = "1" } } },
            ["session"] = "", ["asy_upload"] = 0, ["cmd"] = "FileUpload" };
        var control = new JsonObject { ["control_req"] = new JsonArray(requestEntry) };
        using var controlContent = new StringContent(control.ToJsonString(), Encoding.UTF8, "application/json");
        using var controlResponse = await SendJsonAsync(context, new Uri($"https://h5.qzone.qq.com/webapp/json/sliceUpload/FileBatchControl/{md5}"), controlContent, cancellationToken);
        var root = controlResponse.RootElement;
        if (root.TryGetProperty("ret", out var ret) && ret.TryGetInt32(out var code) && code != 0)
            throw new OperationException(code, root.TryGetProperty("msg", out var msg) ? msg.GetString() : "Album upload session failed.");
        var session = root.TryGetProperty("data", out var data) && data.TryGetProperty("session", out var sessionValue) ? sessionValue.GetString() : null;
        if (string.IsNullOrWhiteSpace(session)) throw new OperationException(-1, "Album upload session is missing.");

        const int sliceSize = 16384;
        for (var offset = 0; offset < bytes.Length; offset += sliceSize)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var length = Math.Min(sliceSize, bytes.Length - offset);
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(context.BotUin.ToString()), "uin"); form.Add(new StringContent("qun"), "appid");
            form.Add(new StringContent(session), "session"); form.Add(new StringContent(offset.ToString()), "offset");
            form.Add(new ByteArrayContent(bytes, offset, length), "data", "blob"); form.Add(new StringContent("0"), "check_type");
            form.Add(new StringContent("0"), "retry"); form.Add(new StringContent((offset / sliceSize).ToString()), "seq");
            form.Add(new StringContent((offset + length).ToString()), "end"); form.Add(new StringContent("FileUpload"), "cmd");
            form.Add(new StringContent(sliceSize.ToString()), "slice_size");
            using var response = await SendJsonAsync(context, new Uri($"https://h5.qzone.qq.com/webapp/json/sliceUpload/FileUpload?seq={offset / sliceSize}&retry=0&offset={offset}&end={offset + length}&total={bytes.Length}&type=form"), form, cancellationToken);
            if (response.RootElement.TryGetProperty("ret", out var sliceRet) && sliceRet.TryGetInt32(out var sliceCode) && sliceCode != 0)
                throw new OperationException(sliceCode, response.RootElement.TryGetProperty("msg", out var sliceMsg) ? sliceMsg.GetString() : "Album upload failed.");
        }
        var mediaId = root.TryGetProperty("data", out data) && data.TryGetProperty("photo_id", out var photo) ? photo.GetString() : md5;
        return new UploadGroupAlbumImageEventResp(new BotGroupAlbumUploadResult { MediaId = mediaId ?? md5, AlbumId = albumId, Url = string.Empty });
    }
    private async Task<JsonDocument> SendJsonAsync(BotContext context, Uri uri, HttpContent content, CancellationToken cancellationToken)
    {
        using var message = CreateRequest(HttpMethod.Post, uri, content);
        var bytes = await SendRequestAsync(context, message, cancellationToken);
        return HttpResponseParser.ParseJson(bytes, "group.upload_album_image");
    }

    protected override Task<HttpRequestMessage> BuildRequestAsync(BotContext context, UploadGroupAlbumImageEventReq request, CancellationToken cancellationToken) =>
        throw new InvalidOperationException("Album uploads use a control request followed by slices.");
    protected override Task<UploadGroupAlbumImageEventResp> ParseResponseAsync(BotContext context, UploadGroupAlbumImageEventReq request, HttpResponseMessage response, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken) =>
        throw new InvalidOperationException("Album uploads parse each slice response individually.");

}
