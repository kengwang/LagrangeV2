using Lagrange.Core.Common;
using System.Text.Json;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Http;
using Lagrange.Core.Services;
using Lagrange.Core.Internal.Events.System;

namespace Lagrange.Core.Internal.Services.Http;

 [HttpServiceAttribute("qzone.get_message_list", "GET", "/proxy/domain/taotao.qzone.qq.com/cgi-bin/emotion_cgi_msglist_v6", "qzone.qq.com", AuthInjection = HttpAuthInjection.UrlBknFromPSkey)]
[EventSubscribe<GetQzoneMessageListEventReq>(Protocols.All)]
internal sealed class QzoneMessageListHttpService : HttpService<GetQzoneMessageListEventReq, GetQzoneMessageListEventResp>
{
    public QzoneMessageListHttpService() : base("qzone.qq.com") { }
    protected override async Task<HttpRequestMessage> BuildRequestAsync(BotContext context, GetQzoneMessageListEventReq request, CancellationToken cancellationToken)
    {
        if (request.UserUin <= 0 || request.Position < 0 || request.Count is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(request));
        var query = $"uin={request.UserUin}&ftype=0&sort=0&pos={request.Position}&num={request.Count}&replynum=100&callback=_preloadCallback&code_version=1&format=jsonp&need_private_comment=1";
        return await CreateGetRequestAsync(context, new Uri($"https://h5.qzone.qq.com/proxy/domain/taotao.qzone.qq.com/cgi-bin/emotion_cgi_msglist_v6?{query}"), cancellationToken);
    }

    protected override Task<GetQzoneMessageListEventResp> ParseResponseAsync(BotContext context, GetQzoneMessageListEventReq request, HttpResponseMessage response, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken)
    {
        using var document = HttpResponseParser.ParseJsonp(payload, "qzone.get_message_list");
        var root = document.RootElement;
        if (root.TryGetProperty("code", out var code) && code.TryGetInt32(out var resultCode) && resultCode != 0)
            throw new HttpServiceException("qzone.get_message_list", root.TryGetProperty("message", out var message) ? message.ToString() : "QZone request failed.", businessCode: resultCode);
        if (!root.TryGetProperty("msglist", out var list) || list.ValueKind != JsonValueKind.Array) throw new HttpServiceException("qzone.get_message_list", "QZone response is missing msglist.");
        var messages = new List<BotQzoneMessage>();
        foreach (var item in list.EnumerateArray())
        {
            var images = new List<string>();
            if (item.TryGetProperty("pic", out var pictures) && pictures.ValueKind == JsonValueKind.Array)
                foreach (var picture in pictures.EnumerateArray())
                    foreach (var field in new[] { "url3", "url2", "url1", "smallurl" })
                        if (picture.TryGetProperty(field, out var image) && image.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(image.GetString())) { images.Add(image.GetString()!); break; }
            messages.Add(new BotQzoneMessage { Id = Text(item, "tid"), Content = Text(item, "content"), Time = Number(item, "created_time"), CommentCount = (int)Number(item, "cmtnum"), IsPrivate = Number(item, "secret") != 0, Images = images });
        }
        return Task.FromResult(new GetQzoneMessageListEventResp(new BotQzoneMessageListResult { Total = (int)Number(root, "total"), Messages = messages }));
    }

    private static string Text(JsonElement value, string name) => value.TryGetProperty(name, out var item) ? item.ToString() : string.Empty;
    private static long Number(JsonElement value, string name) => value.TryGetProperty(name, out var item) && item.TryGetInt64(out var number) ? number : 0;
}
