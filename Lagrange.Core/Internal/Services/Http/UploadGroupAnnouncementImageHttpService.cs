using Lagrange.Core.Common;
using System.Net;
using System.Text;
using System.Text.Json;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Http;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.Http;

[HttpServiceAttribute("group.upload_announcement_image", "POST", "/cgi-bin/announce/upload_img", "qun.qq.com", "web.qun.qq.com")]
[EventSubscribe<UploadGroupAnnouncementImageEventReq>(Protocols.All)]
internal sealed class UploadGroupAnnouncementImageHttpService : HttpService<UploadGroupAnnouncementImageEventReq, UploadGroupAnnouncementImageEventResp>
{
    public UploadGroupAnnouncementImageHttpService() : base("qun.qq.com", "web.qun.qq.com") { }
    protected override async Task<HttpRequestMessage> BuildRequestAsync(BotContext context, UploadGroupAnnouncementImageEventReq request, CancellationToken cancellationToken)
    {
        if (!request.Image.CanRead) throw new ArgumentException("Image stream is not readable.");
        var pskey = await context.HttpSessionContext.GetPSkeyAsync("qun.qq.com", cancellationToken); var skey = await context.HttpSessionContext.GetSkeyAsync(cancellationToken); if (string.IsNullOrWhiteSpace(skey)) throw new HttpServiceException("group.upload_announcement_image", "Group web skey is unavailable.");
        var data = new MemoryStream(); var buffer = new byte[81920]; while (true) { var read = await request.Image.ReadAsync(buffer, cancellationToken); if (read == 0) break; if (data.Length + read > 20 * 1024 * 1024) throw new ArgumentException("Announcement image exceeds 20 MiB."); await data.WriteAsync(buffer.AsMemory(0, read), cancellationToken); } if (data.Length == 0) throw new ArgumentException("Announcement image is empty.");
        var body = new MultipartFormDataContent(); body.Add(new StringContent(ComputeBkn(skey)), "bkn"); body.Add(new StringContent("troopNotice"), "source"); body.Add(new StringContent("0"), "m"); var imageContent = new ByteArrayContent(data.ToArray()); imageContent.Headers.ContentType = new("image/jpeg"); body.Add(imageContent, "pic_up", "image.jpg");
        return new HttpRequestMessage(HttpMethod.Post, "https://web.qun.qq.com/cgi-bin/announce/upload_img") { Content = body };
    }
    protected override Task<UploadGroupAnnouncementImageEventResp> ParseResponseAsync(BotContext context, UploadGroupAnnouncementImageEventReq request, HttpResponseMessage response, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken)
    {
        using var document = HttpResponseParser.ParseJson(payload, "group.upload_announcement_image"); var root = document.RootElement; var code = root.TryGetProperty("ec", out var ec) && ec.TryGetInt32(out var parsed) ? parsed : -1; if (code != 0) throw new HttpServiceException("group.upload_announcement_image", root.TryGetProperty("em", out var message) ? message.ToString() : "Announcement image upload failed.", businessCode: code);
        var metadata = root.TryGetProperty("id", out var idValue) ? idValue.ToString() : string.Empty; if (string.IsNullOrWhiteSpace(metadata)) throw new HttpServiceException("group.upload_announcement_image", "Announcement image metadata is missing."); using var details = JsonDocument.Parse(WebUtility.HtmlDecode(metadata)); var id = Text(details.RootElement, "id"); var width = Number(details.RootElement, "w"); var height = Number(details.RootElement, "h"); if (string.IsNullOrWhiteSpace(id) || width <= 0 || height <= 0 || width > int.MaxValue || height > int.MaxValue) throw new HttpServiceException("group.upload_announcement_image", "Announcement image metadata is invalid."); return Task.FromResult(new UploadGroupAnnouncementImageEventResp(new BotGroupAnnouncementImage { Id = id, Width = (int)width, Height = (int)height }));
    }
    private static string Text(JsonElement value, string name) => value.TryGetProperty(name, out var item) ? item.ToString() : string.Empty;
    private static long Number(JsonElement value, string name) => value.TryGetProperty(name, out var item) && item.TryGetInt64(out var number) ? number : 0;
    private static string ComputeBkn(string skey) { uint hash = 5381; foreach (var c in skey) hash += (hash << 5) + c; return (hash & 0x7FFFFFFF).ToString(); }
}
