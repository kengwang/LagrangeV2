using Lagrange.Core.Common;
using System.Text;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Http;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.Http;

[HttpServiceAttribute("qzone.set_like", "POST", "/proxy/domain/w.qzone.qq.com/cgi-bin/likes", "qzone.qq.com", AuthInjection = HttpAuthInjection.UrlBknFromPSkey)]
[EventSubscribe<SetQzoneLikeEventReq>(Protocols.All)]
internal sealed class QzoneLikeHttpService : HttpService<SetQzoneLikeEventReq, SetQzoneLikeEventResp>
{

    protected override async Task<HttpRequestMessage> BuildRequestAsync(BotContext context, SetQzoneLikeEventReq request, CancellationToken cancellationToken)
    {
        if (request.TargetUin <= 0) throw new ArgumentOutOfRangeException(nameof(request.TargetUin));
        if (string.IsNullOrWhiteSpace(request.MessageId)) throw new ArgumentException("Message id is required.", nameof(request.MessageId));
        var target = $"http://user.qzone.qq.com/{request.TargetUin}/mood/{Uri.EscapeDataString(request.MessageId)}";
        var body = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["qzreferrer"] = $"https://user.qzone.qq.com/{context.BotUin}", ["opuin"] = context.BotUin.ToString(), ["unikey"] = target, ["curkey"] = target,
            ["appid"] = "311", ["typeid"] = "0", ["abstime"] = request.AbsTime.ToString(), ["fid"] = request.MessageId, ["from"] = "1", ["active"] = "0", ["fupdate"] = "1", ["format"] = "json"
        });
        var cgi = request.Like ? "internal_dolike_app" : "internal_unlike_app";
        return await CreateRequestAsync(context, HttpMethod.Post, new Uri($"https://h5.qzone.qq.com/proxy/domain/w.qzone.qq.com/cgi-bin/likes/{cgi}"), body, cancellationToken);
    }

    protected override Task<SetQzoneLikeEventResp> ParseResponseAsync(BotContext context, SetQzoneLikeEventReq request, HttpResponseMessage response, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken)
    {
        using var document = HttpResponseParser.ParseJsonp(payload, "qzone.set_like");
        var root = document.RootElement;
        var code = root.TryGetProperty("code", out var codeElement) && codeElement.TryGetInt32(out var parsedCode) ? parsedCode : 0;
        var subcode = root.TryGetProperty("subcode", out var subElement) && subElement.TryGetInt32(out var parsedSubcode) ? parsedSubcode : 0;
        if (code != 0 || subcode != 0) throw new HttpServiceException("qzone.set_like", root.TryGetProperty("message", out var message) ? message.ToString() : "QZone like request failed.", businessCode: code != 0 ? code : subcode);
        return Task.FromResult(SetQzoneLikeEventResp.Instance);
    }
}
