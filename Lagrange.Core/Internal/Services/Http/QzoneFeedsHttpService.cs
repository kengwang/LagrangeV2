using Lagrange.Core.Common;
using System.Text;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Context;
using Lagrange.Core.Internal.Http;
using Lagrange.Core.Utility;
using Lagrange.Core.Services;
using Lagrange.Core.Internal.Events.System;

namespace Lagrange.Core.Internal.Services.Http;

 [HttpServiceAttribute("qzone.get_feeds", "GET", "/proxy/domain/ic2.qzone.qq.com/cgi-bin/feeds/feeds3_html_more", "qzone.qq.com", AuthInjection = HttpAuthInjection.UrlBknFromPSkey)]
[EventSubscribe<GetQzoneFeedsEventReq>(Protocols.All)]
internal sealed class QzoneFeedsHttpService : HttpService<GetQzoneFeedsEventReq, GetQzoneFeedsEventResp>
{
    protected override async Task<HttpRequestMessage> BuildRequestAsync(BotContext context, GetQzoneFeedsEventReq request, CancellationToken cancellationToken)
    {
        if (request.UserUin <= 0 || request.Page < 1 || request.Count is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(request));
        var query = $"uin={request.UserUin}&scope=0&view=1&filter=all&flag=1&applist=all&pagenum={request.Page}&count={request.Count}&aisortEndTime=0&aisortOffset=0&aisortBeginTime=0&begintime=0&callback=_preloadCallback&format=jsonp&useutf8=1&outputhtmlfeed=1";
        var httpRequest = await CreateGetRequestAsync(context, new Uri($"https://h5.qzone.qq.com/proxy/domain/ic2.qzone.qq.com/cgi-bin/feeds/feeds3_html_more?{query}"), cancellationToken);
        httpRequest.Headers.Referrer = new Uri($"https://user.qzone.qq.com/{request.UserUin}");
        httpRequest.Headers.TryAddWithoutValidation("Origin", "https://user.qzone.qq.com");
        httpRequest.Headers.TryAddWithoutValidation("Accept", "*/*");
        httpRequest.Headers.TryAddWithoutValidation("Accept-Language", "zh-CN,zh;q=0.9");
        return httpRequest;
    }

    protected override async Task<GetQzoneFeedsEventResp> ParseResponseAsync(BotContext context, GetQzoneFeedsEventReq request, HttpResponseMessage response, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken)
    {
        var text = Encoding.UTF8.GetString(payload.Span);
        var start = text.IndexOf('{');
        if (start < 0) throw new HttpServiceException("qzone.get_feeds", "QZone feeds response is invalid.");
        var value = QzoneJsLiteralParser.Parse(text[start..]);
        if (value is not Dictionary<string, object?> root) throw new HttpServiceException("qzone.get_feeds", "QZone feeds response is not an object.");
        var code = Number(root, "code");
        if (code != 0) throw new HttpServiceException("qzone.get_feeds", String(root, "message"), businessCode: (int)code);
        var data = root.GetValueOrDefault("data") as Dictionary<string, object?>;
        var items = data?.GetValueOrDefault("data") as List<object?> ?? root.GetValueOrDefault("data") as List<object?>;
        if (items is null) throw new HttpServiceException("qzone.get_feeds", "QZone feeds data is missing.");
        var feeds = items.OfType<Dictionary<string, object?>>().Select(item => new BotQzoneFeed
        {
            UserId = (long)Number(item, "uin"), Nickname = String(item, "nickname"), Time = (long)Number(item, "abstime"),
            AppId = (int)Number(item, "appid"), Key = String(item, "key", "feedskey"), Html = String(item, "html")
        }).ToList();
        var hasMore = data is not null ? Number(data, "hasmore") != 0 : false;
        return new GetQzoneFeedsEventResp(new BotQzoneFeedResult { Feeds = feeds, HasMore = hasMore });
    }

    private static uint Number(Dictionary<string, object?> value, string key) => value.GetValueOrDefault(key) switch { double d => (uint)d, long l => (uint)l, int i => (uint)i, string s when uint.TryParse(s, out var n) => n, _ => 0 };
    private static string String(Dictionary<string, object?> value, string key, string? fallback = null) => value.GetValueOrDefault(key)?.ToString() ?? (fallback is not null ? value.GetValueOrDefault(fallback)?.ToString() : null) ?? string.Empty;
}
