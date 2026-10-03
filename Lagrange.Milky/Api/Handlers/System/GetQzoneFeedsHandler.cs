using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.System;

[ApiHandler("get_qzone_feeds")]
public sealed class GetQzoneFeedsHandler(BotContext lagrange) : IApiHandler<GetQzoneFeedsHandler.Request, GetQzoneFeedsHandler.Result>
{
    public async ValueTask<MilkyApiResponse<Result>> HandleAsync(Request request, CancellationToken ct)
    {
        var result = await lagrange.GetQzoneFeeds(request.UserId, request.Position + 1, request.Count, ct).WaitAsync(ct);
        return new(new Result(result));
    }
    public sealed class Request(long? userId = null, int position = 0, int count = 20)
    {
        [JsonPropertyName("user_id")] public long? UserId { get; init; } = userId;
        [JsonPropertyName("position")] public int Position { get; init; } = position;
        [JsonPropertyName("count")] public int Count { get; init; } = count;
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
