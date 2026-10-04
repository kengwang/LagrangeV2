using FastEndpoints;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.System;

public sealed class GetQzoneFeedsHandler(BotContext lagrange) : Endpoint<GetQzoneFeedsHandler.Request, MilkyApiResponse<GetQzoneFeedsHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/get_qzone_feeds");
    }
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        // Feeds are read from the bot's own Qzone timeline. The legacy
        // user_id field is accepted for request compatibility but ignored.
        var result = await lagrange.GetQzoneFeeds(null, request.EffectivePage, request.Count, ct).WaitAsync(ct);
        return new(new Result(result));
    }
    public sealed class Request(int pageNum = 1, int count = 10, int? position = null, long? userId = null)
    {
        [JsonPropertyName("page_num")] public int PageNum { get; init; } = pageNum;
        [JsonPropertyName("position")] public int? Position { get; init; } = position;
        [JsonPropertyName("user_id")] public long? UserId { get; init; } = userId;
        [JsonPropertyName("count")] public int Count { get; init; } = count;
        public int EffectivePage => Position is { } offset ? offset + 1 : PageNum;
    }
    public sealed class Result(Lagrange.Core.Common.Response.BotQzoneFeedResult result)
    {
        [JsonPropertyName("has_more")] public bool HasMore { get; } = result.HasMore;
        [JsonPropertyName("feeds")] public Feed[] Feeds { get; } = result.Feeds.Select(x => new Feed(x)).ToArray();
    }
    public sealed class Feed(Lagrange.Core.Common.Response.BotQzoneFeed feed)
    {
        [JsonPropertyName("user_id")] public long UserId { get; } = feed.UserId;
        [JsonPropertyName("nickname")] public string Nickname { get; } = feed.Nickname;
        [JsonPropertyName("time")] public long Time { get; } = feed.Time;
        [JsonPropertyName("app_id")] public int AppId { get; } = feed.AppId;
        [JsonPropertyName("key")] public string Key { get; } = feed.Key;
        [JsonPropertyName("html")] public string Html { get; } = feed.Html;
    }
}
