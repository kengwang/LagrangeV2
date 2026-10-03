using Lagrange.Core.Common;
using System.Text.Json;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Http;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.Http;

[HttpServiceAttribute("qzone.upload_image", "POST", "https://up.qzone.qq.com/cgi-bin/upload/cgi_upload_image", "qzone.qq.com", AuthInjection = HttpAuthInjection.UrlBknFromPSkey | HttpAuthInjection.FormCredentials)]
[EventSubscribe<UploadQzoneImageEventReq>(Protocols.All)]
internal sealed class QzoneUploadHttpService : HttpService<UploadQzoneImageEventReq, UploadQzoneImageEventResp>
{
    public QzoneUploadHttpService() : base("qzone.qq.com") { }
    protected override async Task<HttpRequestMessage> BuildRequestAsync(BotContext context, UploadQzoneImageEventReq request, CancellationToken cancellationToken)
    {
        if (!request.Image.CanRead) throw new ArgumentException("Image stream is not readable.");
        using var data = new MemoryStream(); await request.Image.CopyToAsync(data, cancellationToken);
        var base64 = Convert.ToBase64String(data.ToArray()); if (base64.Length == 0) throw new ArgumentException("Image cannot be empty.");
        var form = new Dictionary<string, string> { ["filename"] = "filename", ["uin"] = context.BotUin.ToString(), ["zzpaneluin"] = context.BotUin.ToString(), ["p_uin"] = context.BotUin.ToString(), ["uploadtype"] = "1", ["albumtype"] = "7", ["exttype"] = "0", ["refer"] = "shuoshuo", ["output_type"] = "jsonhtml", ["charset"] = "utf-8", ["output_charset"] = "utf-8", ["upload_hd"] = "1", ["hd_width"] = "2048", ["hd_height"] = "10000", ["hd_quality"] = "96", ["base64"] = "1", ["jsonhtml_callback"] = "callback", ["picfile"] = base64, ["qzreferrer"] = $"https://user.qzone.qq.com/{context.BotUin}" };
        return await CreateFormRequestAsync(context, HttpMethod.Post, new Uri("https://up.qzone.qq.com/cgi-bin/upload/cgi_upload_image"), form.Select(x => new KeyValuePair<string, string?>(x.Key, x.Value)), cancellationToken);
    }
    protected override Task<UploadQzoneImageEventResp> ParseResponseAsync(BotContext context, UploadQzoneImageEventReq request, HttpResponseMessage response, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken)
    {
        using var document = HttpResponseParser.ParseJsonp(payload, "qzone.upload_image"); var root = document.RootElement;
        var code = root.TryGetProperty("code", out var codeValue) && codeValue.TryGetInt32(out var parsed) ? parsed : 0; if (code != 0) throw new HttpServiceException("qzone.upload_image", root.TryGetProperty("message", out var message) ? message.ToString() : "QZone upload failed.", businessCode: code);
        if (!root.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Object) throw new HttpServiceException("qzone.upload_image", "QZone upload response data is missing.");
        var album = Text(data, "albumid"); var location = Text(data, "lloc"); var url = Text(data, "url"); if (string.IsNullOrWhiteSpace(album) || string.IsNullOrWhiteSpace(location) || string.IsNullOrWhiteSpace(url)) throw new HttpServiceException("qzone.upload_image", "QZone upload response is missing image metadata.");
        var type = Number(data, "type"); var height = Number(data, "height"); var width = Number(data, "width");
        return Task.FromResult(new UploadQzoneImageEventResp(new BotQzoneUploadResult { RichValue = $",{album},{location},{location},{type},{height},{width},,{height},{width}", Url = url, AlbumId = album, Location = location, Type = type, Width = width, Height = height }));
    }
    private static string Text(JsonElement value, string name) => value.TryGetProperty(name, out var item) ? item.ToString() : string.Empty;
    private static int Number(JsonElement value, string name) => value.TryGetProperty(name, out var item) && item.TryGetInt32(out var number) ? number : 0;
}
