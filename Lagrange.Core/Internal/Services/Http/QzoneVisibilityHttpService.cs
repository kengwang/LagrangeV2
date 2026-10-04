using System.Text.Json;
using System.Web;
using Lagrange.Core.Common;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Http;
using Lagrange.Core.Services;
namespace Lagrange.Core.Internal.Services.Http;
[HttpServiceAttribute("qzone.visibility", "POST", "/proxy/domain", "qzone.qq.com", AuthInjection = HttpAuthInjection.UrlBknFromPSkey)]
[EventSubscribe<QzoneVisibilityEventReq>(Protocols.All)]
internal sealed class QzoneVisibilityHttpService : HttpService<QzoneVisibilityEventReq, QzoneVisibilityEventResp>
{
    protected override async Task<HttpRequestMessage> BuildRequestAsync(BotContext context, QzoneVisibilityEventReq request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.MessageId);
        if (request.Right is not (1 or 4 or 16 or 64 or 128)) throw new ArgumentOutOfRangeException(nameof(request.Right));
        if (request.Right is 16 or 128 && (request.Users.Count == 0 || request.Users.Any(x => x <= 0))) throw new ArgumentException("Selected visibility requires positive user identifiers.");
        var uri = new Uri($"https://h5.qzone.qq.com/proxy/domain/taotao.qq.com/cgi-bin/emotion_cgi_msgdetail_v6?tid={Uri.EscapeDataString(request.MessageId)}&uin={context.BotUin}&t1_source=1&not_trunc_con=1&need_right=1&not_adapt_outpic=1");
        using var detailRequest = await CreateGetRequestAsync(context, uri, cancellationToken);
        var bytes = await SendRequestAsync(context, detailRequest, cancellationToken);
        using var document = HttpResponseParser.ParseJsonp(bytes, "qzone.visibility.detail");
        ValidateResponse(document.RootElement);
        var fields = BuildUpdateFields(document.RootElement, context.BotUin, request.MessageId);
        fields["ugc_right"] = request.Right.ToString();
        if (request.Right is 16 or 128) fields["allow_uins"] = string.Join('|', request.Users.Distinct());
        return await CreateFormRequestAsync(context, HttpMethod.Post, new Uri("https://h5.qzone.qq.com/proxy/domain/taotao.qzone.qq.com/cgi-bin/emotion_cgi_update"), fields.Select(x => new KeyValuePair<string, string?>(x.Key, x.Value)), cancellationToken);
    }
    internal static Dictionary<string, string> BuildUpdateFields(JsonElement detail, long self, string expectedId)
    {
        static string Text(JsonElement e, string key, string fallback = "") => e.TryGetProperty(key, out var v) ? v.ToString() : fallback;
        if (Text(detail, "tid") != expectedId || Text(detail, "uin") != self.ToString()) throw new InvalidOperationException("Qzone detail does not belong to the requested account and post.");
        var rich = new List<string>(); var bos = new List<string>();
        if (detail.TryGetProperty("pic", out var pictures) && pictures.ValueKind == JsonValueKind.Array)
            foreach (var picture in pictures.EnumerateArray())
            {
                var parts = Text(picture, "pic_id").Split(',');
                if (parts.Length < 3 || parts[1].Length == 0 || parts[2].Length == 0) throw new InvalidOperationException("Cannot preserve incomplete Qzone picture metadata.");
                rich.Add($",{parts[1]},{parts[2]},{parts[2]},{Text(picture, "pictype", Text(picture, "type", "22"))},{Text(picture, "height", Text(picture, "b_height", "0"))},{Text(picture, "width", Text(picture, "b_width", "0"))},,0,0");
                foreach (var field in new[] { "smallurl", "url1", "url2", "url3" })
                    if (Uri.TryCreate(Text(picture, field), UriKind.Absolute, out var url) && HttpUtility.ParseQueryString(url.Query)["bo"] is { } bo) { bos.Add(bo); break; }
            }
        var richType = Text(detail, "richtype", rich.Count == 0 ? "" : "1");
        if (richType == "1" && rich.Count == 0) throw new InvalidOperationException("Image post has no preservable picture metadata.");
        var content = Text(detail, "content");
        if (detail.TryGetProperty("conlist", out var conlist) && conlist.ValueKind == JsonValueKind.Array && conlist.GetArrayLength() > 0)
            content = string.Concat(conlist.EnumerateArray().Select(x => Text(x, "con")));
        var boGroup = string.Join(',', bos);
        return new Dictionary<string, string>
        {
            ["syn_tweet_verson"] = "1", ["tid"] = expectedId, ["paramstr"] = "1", ["pic_template"] = Text(detail, "pic_template"),
            ["richtype"] = richType, ["richval"] = rich.Count > 0 ? string.Join('\t', rich) : Text(detail, "richval"),
            ["special_url"] = Text(detail, "special_url"), ["subrichtype"] = Text(detail, "t1_subtype", Text(detail, "subrichtype", rich.Count == 0 ? "" : "1")),
            ["pic_bo"] = boGroup.Length == 0 ? "" : $"{boGroup}\t{boGroup}", ["con"] = content,
            ["feedversion"] = Text(detail, "feedversion", "1"), ["ver"] = Text(detail, "ver", "1"), ["to_sign"] = Text(detail, "to_sign", "0"),
            ["ugcright_id"] = Text(detail, "ugcright_id", expectedId), ["hostuin"] = self.ToString(), ["code_version"] = Text(detail, "code_version", "1"),
            ["format"] = "fs", ["qzreferrer"] = $"https://user.qzone.qq.com/{self}"
        };
    }
    private static void ValidateResponse(JsonElement root)
    {
        foreach (var key in new[] { "code", "subcode" })
            if (root.TryGetProperty(key, out var value) && value.TryGetInt32(out var code) && code != 0)
                throw new HttpServiceException("qzone.visibility", "Qzone visibility request failed.", businessCode: code);
    }
    protected override Task<QzoneVisibilityEventResp> ParseResponseAsync(BotContext context, QzoneVisibilityEventReq request, HttpResponseMessage response, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken)
    {
        using var document = HttpResponseParser.ParseJsonp(payload, "qzone.visibility");
        ValidateResponse(document.RootElement);
        if (!document.RootElement.TryGetProperty("code", out var code) || !code.TryGetInt32(out var status) || status != 0)
            throw new HttpServiceException("qzone.visibility", "Qzone returned no successful visibility acknowledgement.");
        return Task.FromResult(new QzoneVisibilityEventResp());
    }
}
