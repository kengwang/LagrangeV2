using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Lagrange.Core.Common;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Services;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Internal.Services.Http;

[HttpServiceAttribute("qzone.web", "POST", "/proxy/domain", "qzone.qq.com", AuthInjection = HttpAuthInjection.UrlBknFromPSkey)]
[EventSubscribe<QzoneWebEvent>(Protocols.All)]
internal sealed class QzoneWebHttpService : HttpService<QzoneWebEvent, QzoneRpcResult>
{
    protected override async Task<HttpRequestMessage> BuildRequestAsync(BotContext context, QzoneWebEvent request, CancellationToken ct)
    {
        string endpoint;
        var fields = new Dictionary<string, string?>();
        switch (request.Operation)
        {
            case QzoneWebOperation.GetProfile:
                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(request.TargetUin);
                return await CreateGetRequestAsync(context, new Uri($"https://h5.qzone.qq.com/proxy/domain/base.qzone.qq.com/cgi-bin/user/cgi_userinfo_get_all?uin={request.TargetUin}&vuin={context.BotUin}&fupdate=1"), ct);
            case QzoneWebOperation.SetProfile:
                fields = ProfileFields(context.BotUin, request.Name, request.Description);
                endpoint = "https://h5.qzone.qq.com/proxy/domain/w.qzone.qq.com/cgi-bin/user/cgi_apply_updateuserinfo_new";
                break;
            case QzoneWebOperation.GetDetail:
                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(request.TargetUin);
                ArgumentException.ThrowIfNullOrWhiteSpace(request.PostId);
                if (request.Position < 0 || request.Count is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(request.Count));
                return await CreateGetRequestAsync(context, new Uri($"https://h5.qzone.qq.com/proxy/domain/taotao.qq.com/cgi-bin/emotion_cgi_msgdetail_v6?tid={Uri.EscapeDataString(request.PostId)}&uin={request.TargetUin}&t1_source=1&not_trunc_con=1&need_right=1&not_adapt_outpic=1&pos={request.Position}&num={request.Count}"), ct);
            case QzoneWebOperation.Reply:
                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(request.TargetUin);
                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(request.CommentUin);
                ArgumentException.ThrowIfNullOrWhiteSpace(request.PostId);
                ArgumentException.ThrowIfNullOrWhiteSpace(request.CommentId);
                ArgumentException.ThrowIfNullOrWhiteSpace(request.Content);
                string content = request.Content;
                if (request.ReplyUin is { } replyUin)
                {
                    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(replyUin);
                    ArgumentException.ThrowIfNullOrWhiteSpace(request.ReplyName);
                    var name = request.ReplyName.Trim().Replace("%", "%25").Replace(",", "%2C").Replace("}", "%7D");
                    content = $"@{{uin:{replyUin},nick:{name},auto:1}} {content}";
                }
                else if (request.ReplyName is not null) throw new ArgumentException("reply_name requires reply_uin.");
                fields = new() { ["uin"] = context.BotUin.ToString(), ["hostUin"] = request.TargetUin.ToString(), ["hostuin"] = context.BotUin.ToString(),
                    ["topicId"] = $"{request.TargetUin}_{request.PostId}", ["commentId"] = request.CommentId, ["commentUin"] = request.CommentUin.ToString(),
                    ["content"] = content, ["inCharset"] = "utf-8", ["outCharset"] = "utf-8", ["ref"] = "", ["code_version"] = "1", ["format"] = "fs", ["qzreferrer"] = $"https://user.qzone.qq.com/{request.TargetUin}" };
                endpoint = "https://h5.qzone.qq.com/proxy/domain/taotao.qzone.qq.com/cgi-bin/emotion_cgi_addreply_ugc";
                break;
            default: throw new ArgumentOutOfRangeException(nameof(request.Operation));
        }
        return await CreateFormRequestAsync(context, HttpMethod.Post, new Uri(endpoint), fields, ct);
    }

    internal static Dictionary<string, string?> ProfileFields(long selfUin, string? name, string? description)
    {
        if (name is null && description is null) throw new ArgumentException("At least one Qzone profile field is required.");
        var fields = new Dictionary<string, string?> { ["uin"] = selfUin.ToString(), ["pageindex"] = "3", ["fupdate"] = "1" };
        int bitmap = 0;
        if (name is not null) { fields["spacename"] = name.Replace("\r", ""); bitmap |= 2048; }
        if (description is not null) { fields["desc"] = description.Replace("\r", ""); bitmap |= 4096; }
        fields["mb"] = bitmap.ToString();
        return fields;
    }

    protected override Task<QzoneRpcResult> ParseResponseAsync(BotContext context, QzoneWebEvent request, HttpResponseMessage response, ReadOnlyMemory<byte> payload, CancellationToken ct)
    {
        var root = QzoneJsLiteralParser.ParseJsonResponse(Encoding.UTF8.GetString(payload.Span));
        int code = root["code"]?.GetValue<int>() ?? throw new InvalidDataException("Qzone response has no business status.");
        int subcode = root["subcode"]?.GetValue<int>() ?? 0;
        if (code != 0 || subcode != 0) throw new HttpServiceException("qzone.web", root["message"]?.ToString() ?? "Qzone request failed.", businessCode: code != 0 ? code : subcode);
        JsonObject result;
        switch (request.Operation)
        {
            case QzoneWebOperation.GetProfile:
                var data = root["data"]?.AsObject() ?? throw new InvalidDataException("Qzone profile data is missing.");
                result = new JsonObject { ["name"] = data["spacename"]?.GetValue<string>() ?? throw new InvalidDataException("Qzone name is missing."), ["description"] = data["desc"]?.GetValue<string>() ?? throw new InvalidDataException("Qzone description is missing.") };
                break;
            case QzoneWebOperation.GetDetail: result = ReadDetail(root, request.PostId); break;
            case QzoneWebOperation.Reply:
                result = ReadReply(root, request.CommentId, request.CommentUin);
                break;
            default: result = new JsonObject(); break;
        }
        using var document = JsonDocument.Parse(result.ToJsonString());
        return Task.FromResult(new QzoneRpcResult(document.RootElement.Clone()));
    }

    internal static JsonObject ReadReply(JsonObject response, string parentId, long parentUin)
    {
        var id = response["data"]?["id"];
        if (id is not JsonValue || id.GetValueKind() is not (JsonValueKind.String or JsonValueKind.Number) || string.IsNullOrWhiteSpace(id.ToString()))
            throw new InvalidDataException("Qzone reply has no confirmation identifier.");
        return new JsonObject { ["comment_id"] = id.ToString(), ["parent_comment_id"] = parentId, ["parent_comment_uin"] = parentUin };
    }

    internal static JsonObject ReadDetail(JsonObject data, string expectedId)
    {
        var id = data["tid"]?.GetValue<string>();
        if (id != expectedId) throw new InvalidDataException("Qzone detail returned a different post.");
        var comments = new JsonArray();
        foreach (var node in data["commentlist"]?.AsArray() ?? [])
        {
            var comment = node?.AsObject() ?? throw new InvalidDataException("Qzone comment is invalid.");
            var mapped = Comment(comment);
            var replies = new JsonArray();
            foreach (var reply in comment["list_3"]?.AsArray() ?? [])
            {
                var entry = Comment(reply?.AsObject() ?? throw new InvalidDataException("Qzone reply is invalid."));
                entry["parent_comment_id"] = mapped["comment_id"]!.DeepClone(); entry["parent_comment_uin"] = mapped["uin"]!.DeepClone(); replies.Add((JsonNode)entry);
            }
            mapped["replies"] = replies; comments.Add((JsonNode)mapped);
        }
        return new JsonObject { ["tid"] = id, ["content"] = data["content"]?.GetValue<string>() ?? throw new InvalidDataException("Qzone post content is missing."),
            ["comment_num"] = data["cmtnum"]?.DeepClone() ?? throw new InvalidDataException("Qzone comment count is missing."), ["comments"] = comments };
    }

    private static JsonObject Comment(JsonObject value) => new()
    {
        ["comment_id"] = value["tid"]?.ToString() ?? throw new InvalidDataException("Qzone comment identifier is missing."),
        ["uin"] = value["uin"]?.DeepClone() ?? throw new InvalidDataException("Qzone comment author is missing."),
        ["name"] = value["name"]?.GetValue<string>() ?? throw new InvalidDataException("Qzone comment author name is missing."),
        ["time"] = value["create_time"]?.DeepClone() ?? throw new InvalidDataException("Qzone comment time is missing."),
        ["content"] = value["content"]?.GetValue<string>() ?? throw new InvalidDataException("Qzone comment content is missing.")
    };
}
