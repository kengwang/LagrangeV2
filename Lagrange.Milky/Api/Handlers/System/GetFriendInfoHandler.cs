using FastEndpoints;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Converters;

namespace Lagrange.Milky.Api.Handlers.System;

public sealed class GetFriendInfoHandler(BotContext lagrange, MilkyConverter converter) : Endpoint<GetFriendInfoHandler.Request, MilkyApiResponse<GetFriendInfoHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/get_friend_info");
    }
    private readonly BotContext _lagrange = lagrange;
    private readonly MilkyConverter _converter = converter;

    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        var friend = (await _lagrange.FetchFriends(request.NoCache).WaitAsync(ct))
            .FirstOrDefault(f => f.Uin == request.UserId);

        return friend == null
            ? new MilkyApiResponse<Result>(-404, "Firend not found")
            : new MilkyApiResponse<Result>(new Result
            {
                Friend = _converter.ToFriend(friend)
            });
    }

    public sealed class Request(long userId, bool noCache = false)
    {
        [JsonPropertyName("user_id")] public required long UserId { get; init; } = userId;
        [JsonPropertyName("no_cache")] public bool NoCache { get; init; } = noCache;
    }

    public sealed class Result
    {
        [JsonPropertyName("friend")] public required Lagrange.Milky.Models.Friend Friend { get; init; }
    }
}
