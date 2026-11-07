using System.Globalization;
using System.Text;
using System.Text.Json.Nodes;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Http;
using Lagrange.Core.Services;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Internal.Services.Http;

[HttpServiceAttribute("qzone.get_feeds", "GET", "/proxy/domain/ic2.qzone.qq.com/cgi-bin/feeds/feeds3_html_more", "qzone.qq.com", AuthInjection = HttpAuthInjection.UrlBknFromPSkey)]
[EventSubscribe<GetQzoneFeedsEventReq>(Protocols.All)]
internal sealed class QzoneFeedsHttpService : HttpService<GetQzoneFeedsEventReq, GetQzoneFeedsEventResp>
{
    private static readonly string[] NumericFields = ["begintime", "dayspac", "pagenum", "aisortEndTime", "aisortOffset", "aisortBeginTime", "aisortNextTime", "icServerTime"];
    private static readonly string[] StringFields = ["externparam", "daylist", "uinlist"];

    internal static JsonObject Initial(long uin)
    {
        var state = new JsonObject { ["version"] = 1, ["kind"] = "friends", ["uin"] = uin.ToString(CultureInfo.InvariantCulture), ["page"] = 1 };
        foreach (var field in NumericFields) state[field] = field == "pagenum" ? "1" : "0";
        foreach (var field in StringFields) state[field] = "";
        return state;
    }

    public override async Task<GetQzoneFeedsEventResp> ExecuteAsync(BotContext context, GetQzoneFeedsEventReq request, CancellationToken ct = default)
    {
        if (request.UserUin <= 0 || request.Page < 1 || request.Count is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(request));
        var state = request.Cursor is null ? Initial(context.BotUin) : QzonePublicExt.DecodeCursor(request.Cursor, context.BotUin, "friends");
        var start = state["page"]!.GetValue<int>();
        if (start < 1 || request.Cursor is not null && request.Page != 1 && request.Page != start) throw new ArgumentException("Qzone page and cursor do not match.");
        int target = request.Cursor is null ? request.Page : start;
        var seen = new HashSet<string>(StringComparer.Ordinal);
        for (int page = start; page <= target; page++)
        {
            ct.ThrowIfCancellationRequested();
            if (!seen.Add(Position(state))) throw new InvalidDataException("Qzone pagination repeated a cursor.");
            var current = new GetQzoneFeedsEventReq(request.UserUin, page, request.Count) { State = state };
            var response = await base.ExecuteAsync(context, current, ct);
            if (page == target) return response;
            if (!response.Result.HasMore) return new GetQzoneFeedsEventResp(new BotQzoneFeedResult { Feeds = [], HasMore = false });
            state = QzonePublicExt.DecodeCursor(response.Result.NextCursor!, context.BotUin, "friends");
        }
        throw new InvalidDataException("Qzone pagination has no requested page.");
    }

    protected override async Task<HttpRequestMessage> BuildRequestAsync(BotContext context, GetQzoneFeedsEventReq request, CancellationToken ct)
    {
        var state = request.State ?? Initial(context.BotUin);
        var query = new Dictionary<string, string> { ["uin"] = request.UserUin.ToString(), ["scope"] = "0", ["view"] = "1", ["filter"] = "all", ["flag"] = "1", ["applist"] = "all", ["refresh"] = "0",
            ["count"] = request.Count.ToString(), ["callback"] = "_preloadCallback", ["format"] = "jsonp", ["useutf8"] = "1", ["outputhtmlfeed"] = "1" };
        foreach (var field in NumericFields.Concat(StringFields)) query[field == "aisortNextTime" ? "getAisort" : field] = state[field]?.GetValue<string>() ?? throw new ArgumentException("Qzone cursor field is missing.");
        var uri = new Uri("https://h5.qzone.qq.com/proxy/domain/ic2.qzone.qq.com/cgi-bin/feeds/feeds3_html_more?" + string.Join('&', query.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}")));
        var message = await CreateGetRequestAsync(context, uri, ct);
        message.Headers.Referrer = new Uri($"https://user.qzone.qq.com/{request.UserUin}");
        message.Headers.Add("Origin", "https://user.qzone.qq.com");
        return message;
    }

    protected override Task<GetQzoneFeedsEventResp> ParseResponseAsync(BotContext context, GetQzoneFeedsEventReq request, HttpResponseMessage response, ReadOnlyMemory<byte> payload, CancellationToken ct)
    {
        var root = QzoneJsLiteralParser.ParseJsonResponse(Encoding.UTF8.GetString(payload.Span));
        int code = root["code"]?.GetValue<int>() ?? throw new InvalidDataException("Qzone feed response has no status.");
        int subcode = root["subcode"]?.GetValue<int>() ?? 0;
        if (code != 0 || subcode != 0) throw new HttpServiceException("qzone.get_feeds", root["message"]?.ToString() ?? "Qzone feed request failed.", businessCode: code != 0 ? code : subcode);
        var data = root["data"];
        var items = data is JsonArray array ? array : data?["data"]?.AsArray() ?? throw new InvalidDataException("Qzone feed list is missing.");
        var main = data is JsonObject obj ? obj["main"]?.AsObject() : null;
        if (main?["error"] is { } error && error.ToString().Length > 0) throw new InvalidDataException("Qzone feed server reported a paging error.");
        bool more = ReadMore(main is null ? data is JsonObject d ? d["hasmore"] : root["hasmore"] : main["hasMoreFeeds"]);
        var feeds = items.Where(node => node is not null).Select(node =>
        {
            var item = node?.AsObject() ?? throw new InvalidDataException("Qzone feed entry is invalid.");
            var feed = new BotQzoneFeed { UserId = Number(item["uin"]), Nickname = item["nickname"]?.GetValue<string>() ?? "", Time = Number(item["abstime"]),
                AppId = checked((int)Number(item["appid"])), Key = item["key"]?.ToString() ?? item["feedskey"]?.ToString() ?? "", Html = item["html"]?.GetValue<string>() ?? "",
                LikeCount = OptionalCount(item["likecnt"]), CommentCount = OptionalCount(item["commentcnt"]) };
            QzoneFeedContent.Populate(feed);
            return feed;
        }).ToArray();
        string? cursor = null;
        if (more)
        {
            if (main is null) throw new InvalidDataException("Qzone feed response has no pagination state.");
            var state = NextState(main, request.State ?? Initial(context.BotUin));
            state["page"] = checked(request.Page + 1);
            cursor = QzonePublicExt.EncodeCursor(state);
        }
        return Task.FromResult(new GetQzoneFeedsEventResp(new BotQzoneFeedResult { Feeds = feeds, HasMore = more, NextCursor = cursor }));
    }

    private static bool ReadMore(JsonNode? value) => value?.ToString() switch { "true" or "1" => true, "false" or "0" => false, _ => throw new InvalidDataException("Qzone pagination status is invalid.") };
    private static long Number(JsonNode? node) => node is not null && long.TryParse(node.ToString(), NumberStyles.None, CultureInfo.InvariantCulture, out var number) ? number : throw new InvalidDataException("Qzone numeric field is invalid.");
    private static long? OptionalCount(JsonNode? node) => node is null || node.ToString().Length == 0 ? null : Number(node);
    private static string Position(JsonObject state) => new JsonArray(NumericFields.Where(x => x != "icServerTime").Concat(StringFields).Select(x => (JsonNode?)JsonValue.Create(state[x]?.ToString())).ToArray()).ToJsonString();

    internal static JsonObject NextState(JsonObject main, JsonObject previous)
    {
        var state = (JsonObject)previous.DeepClone();
        foreach (var field in NumericFields)
        {
            if (main[field] is not { } value || value.ToString().Length == 0) continue;
            long number = Number(value);
            if (field == "pagenum" && number < 1) throw new InvalidDataException("Qzone page number is invalid.");
            state[field] = number.ToString(CultureInfo.InvariantCulture);
        }
        foreach (var field in StringFields)
        {
            if (main[field] is not { } value) continue;
            var text = value.GetValue<string>();
            if (text.Length > 32768) throw new InvalidDataException("Qzone pagination field is too large.");
            if (text.Length > 0 || field == "uinlist") state[field] = text;
        }
        if (Position(previous) == Position(state)) throw new InvalidDataException("Qzone pagination did not advance.");
        return state;
    }
}
