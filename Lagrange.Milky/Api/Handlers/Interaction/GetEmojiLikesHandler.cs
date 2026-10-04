using FastEndpoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.Interaction;

public sealed class GetEmojiLikesHandler(BotContext lagrange) : Endpoint<GetEmojiLikesHandler.Request, MilkyApiResponse<GetEmojiLikesHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/get_emoji_likes");
    }
    private readonly BotContext _lagrange = lagrange;

    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Code);
        if (request.Count is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(request.Count));
        var result = await _lagrange.GetEmojiLikes(request.GroupId, checked((ulong)request.MessageSeq), request.Code, request.Cookie, checked((uint)request.Count), ct).WaitAsync(ct);
        return new MilkyApiResponse<Result>(new Result
        {
            Users = [.. result.Users.Select(user => new LikeUser { UserId = user.Uin, Nickname = user.Nickname, HeadUrl = user.HeadUrl })],
            Cookie = result.Cookie,
            IsLast = result.IsLast,
        });
    }

    public sealed class Request(long groupId, long messageSeq, string code, string cookie = "", int count = 20)
    {
        [JsonPropertyName("group_id")] public required long GroupId { get; init; } = groupId;
        [JsonPropertyName("message_seq")] public required long MessageSeq { get; init; } = messageSeq;
        [JsonPropertyName("code")] public required string Code { get; init; } = code;
        [JsonPropertyName("cookie")] public string Cookie { get; init; } = cookie;
        [JsonPropertyName("count")] public int Count { get; init; } = count;
    }

    public sealed class Result
    {
        [JsonPropertyName("users")] public required IReadOnlyList<LikeUser> Users { get; init; }
        [JsonPropertyName("cookie")] public required string Cookie { get; init; }
        [JsonPropertyName("is_last")] public required bool IsLast { get; init; }
    }

    public sealed class LikeUser
    {
        [JsonPropertyName("user_id")] public required long UserId { get; init; }
        [JsonPropertyName("nickname")] public required string Nickname { get; init; }
        [JsonPropertyName("head_url")] public required string HeadUrl { get; init; }
    }
}
