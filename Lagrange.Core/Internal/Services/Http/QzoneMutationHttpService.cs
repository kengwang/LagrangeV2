using Lagrange.Core.Common;
using System.Text;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Http;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.Http;

[HttpServiceAttribute("qzone.mutation", "POST", "/proxy/domain", "qzone.qq.com", AuthInjection = HttpAuthInjection.UrlBknFromPSkey)]
[EventSubscribe<QzoneMutationEventReq>(Protocols.All)]
internal sealed class QzoneMutationHttpService : HttpService<QzoneMutationEventReq, QzoneMutationEventResp>
{
    public QzoneMutationHttpService() : base("qzone.qq.com") { }

    protected override async Task<HttpRequestMessage> BuildRequestAsync(BotContext context, QzoneMutationEventReq request, CancellationToken cancellationToken)
    {
        if (request.Kind == QzoneMutationKind.Comment && (request.TargetUin <= 0 || string.IsNullOrWhiteSpace(request.MessageId) || string.IsNullOrWhiteSpace(request.Content))) throw new ArgumentException("Comment target, message and content are required.");
        if (request.Kind == QzoneMutationKind.Delete && string.IsNullOrWhiteSpace(request.MessageId)) throw new ArgumentException("Message id is required.");
        if (request.Kind == QzoneMutationKind.Publish && string.IsNullOrWhiteSpace(request.Content)) throw new ArgumentException("Message content is required.");
        if (request.Kind == QzoneMutationKind.Black && request.TargetUin <= 0) throw new ArgumentOutOfRangeException(nameof(request.TargetUin));
        Dictionary<string, string> fields; string endpoint;
        switch (request.Kind)
        {
            case QzoneMutationKind.Comment:
                endpoint = "https://h5.qzone.qq.com/proxy/domain/taotao.qzone.qq.com/cgi-bin/emotion_cgi_re_feeds";
                fields = new() { ["qzreferrer"] = $"https://user.qzone.qq.com/{context.BotUin}", ["inCharset"] = "utf-8", ["outCharset"] = "utf-8", ["hostUin"] = request.TargetUin.ToString(), ["format"] = "json", ["ref"] = "feeds", ["topicId"] = $"{request.TargetUin}_{request.MessageId}__1", ["feedsType"] = "100", ["private"] = "0", ["paramstr"] = "1", ["uin"] = context.BotUin.ToString(), ["content"] = request.Content, ["plat"] = "qzone", ["source"] = "ic", ["platformid"] = "52" };
                break;
            case QzoneMutationKind.Delete:
                endpoint = "https://h5.qzone.qq.com/proxy/domain/taotao.qzone.qq.com/cgi-bin/emotion_cgi_delete_v6";
                fields = new() { ["hostuin"] = context.BotUin.ToString(), ["tid"] = request.MessageId, ["t1_source"] = "1", ["code_version"] = "1", ["format"] = "fs", ["qzreferrer"] = $"https://user.qzone.qq.com/{context.BotUin}" };
                break;
            case QzoneMutationKind.Publish:
                endpoint = "https://h5.qzone.qq.com/proxy/domain/taotao.qq.com/cgi-bin/emotion_cgi_publish_v6";
                fields = new() { ["syn_tweet_verson"] = "1", ["paramstr"] = string.IsNullOrWhiteSpace(request.RichValue) ? "1" : "0", ["richtype"] = string.IsNullOrWhiteSpace(request.RichValue) ? "" : "1", ["richval"] = request.RichValue, ["con"] = request.Content, ["feedversion"] = "1", ["ver"] = "1", ["ugc_right"] = "1", ["to_sign"] = "0", ["who"] = "1", ["hostuin"] = context.BotUin.ToString(), ["code_version"] = "1", ["format"] = "json", ["qzreferrer"] = $"https://user.qzone.qq.com/{context.BotUin}" };
                break;
            default:
                endpoint = "https://h5.qzone.qq.com/proxy/domain/w.qzone.qq.com/cgi-bin/right/cgi_black_action_new";
                fields = new() { ["uin"] = context.BotUin.ToString(), ["act_uin"] = request.TargetUin.ToString(), ["action"] = request.Ban ? "1" : "2", ["fupdate"] = "1", ["qzreferrer"] = $"https://user.qzone.qq.com/{context.BotUin}/main" };
                break;
        }
        return await CreateFormRequestAsync(context, HttpMethod.Post, new Uri(endpoint), fields.Select(pair => new KeyValuePair<string, string?>(pair.Key, pair.Value)), cancellationToken);
    }

    protected override Task<QzoneMutationEventResp> ParseResponseAsync(BotContext context, QzoneMutationEventReq request, HttpResponseMessage response, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken)
    {
        using var document = HttpResponseParser.ParseJsonp(payload, "qzone.mutation");
        var root = document.RootElement;
        var code = root.TryGetProperty("code", out var c) && c.TryGetInt32(out var cv) ? cv : 0;
        var subcode = root.TryGetProperty("subcode", out var s) && s.TryGetInt32(out var sv) ? sv : 0;
        if (code != 0 || subcode != 0) throw new HttpServiceException("qzone.mutation", root.TryGetProperty("message", out var m) ? m.ToString() : "QZone mutation failed.", businessCode: code != 0 ? code : subcode);
        if (request.Kind == QzoneMutationKind.Comment) { var id = root.TryGetProperty("commentid", out var cid) ? cid.ToString() : root.TryGetProperty("commentId", out cid) ? cid.ToString() : string.Empty; return Task.FromResult(new QzoneMutationEventResp(new BotQzoneCommentResult { CommentId = id }, null)); }
        if (request.Kind == QzoneMutationKind.Publish) { var id = root.TryGetProperty("t1_tid", out var tid) ? tid.ToString() : root.TryGetProperty("tid", out tid) ? tid.ToString() : string.Empty; if (string.IsNullOrWhiteSpace(id)) throw new HttpServiceException("qzone.mutation", "QZone publish response is missing tid."); var time = root.TryGetProperty("t1_time", out var timeValue) && timeValue.TryGetInt64(out var parsed) ? parsed : 0; return Task.FromResult(new QzoneMutationEventResp(null, new BotQzonePublishResult { MessageId = id, Time = time })); }
        return Task.FromResult(new QzoneMutationEventResp(null, null));
    }
}
