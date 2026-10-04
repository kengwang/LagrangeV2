using FastEndpoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.System;

public sealed class GetProfileLikeHandler(BotContext lagrange) : Endpoint<GetProfileLikeHandler.Request, MilkyApiResponse<GetProfileLikeHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/get_like");
    }
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        if (request.UserId is <= 0) throw new ArgumentOutOfRangeException(nameof(request.UserId));
        var result = await lagrange.GetProfileLike(request.UserId, request.Start, request.Limit, ct).WaitAsync(ct);
        return new(new Result(result));
    }

    public sealed class Request(long? userId = null, int start = 0, int limit = 10)
    {
        [JsonPropertyName("user_id")] public long? UserId { get; init; } = userId;
        [JsonPropertyName("start")] public int Start { get; init; } = start;
        [JsonPropertyName("limit")] public int Limit { get; init; } = limit;
    }

    public sealed class Result(Lagrange.Core.Common.Response.BotProfileLikeResult result)
    {
        [JsonPropertyName("uid")] public string Uid { get; } = result.Uid;
        [JsonPropertyName("time")] public long Time { get; } = result.Time;
        [JsonPropertyName("total_count")] public int TotalCount { get; } = result.TotalCount;
        [JsonPropertyName("new_count")] public int NewCount { get; } = result.NewCount;
        [JsonPropertyName("users")] public IReadOnlyList<ProfileLikeUser> Users { get; } = result.Users.Select(x => new ProfileLikeUser(x)).ToArray();
    }

    public sealed class ProfileLikeUser(Lagrange.Core.Common.Response.BotProfileLikeUser user)
    {
        [JsonPropertyName("uid")] public string Uid { get; } = user.Uid;
        [JsonPropertyName("user_id")] public long UserId { get; } = user.Uin;
        [JsonPropertyName("nickname")] public string Nickname { get; } = user.Nickname;
        [JsonPropertyName("latest_time")] public long LatestTime { get; } = user.LatestTime;
        [JsonPropertyName("count")] public int Count { get; } = user.Count;
        [JsonPropertyName("is_friend")] public bool IsFriend { get; } = user.IsFriend;
    }
}
