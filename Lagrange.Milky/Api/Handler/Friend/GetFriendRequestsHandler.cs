using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Utility;

namespace Lagrange.Milky.Api.Handler.Friend;

[Api("get_friend_requests")]
public class GetFriendRequestsHandler(BotContext bot, EntityConvert convert) : IApiHandler<GetFriendRequestsParameter, GetFriendRequestsResult>
{
    private readonly BotContext _bot = bot;
    private readonly EntityConvert _convert = convert;

    public async Task<GetFriendRequestsResult> HandleAsync(GetFriendRequestsParameter parameter, CancellationToken token)
    {
        var requests = await _bot.FetchFriendRequests();

        return new GetFriendRequestsResult(requests
            .Take(parameter.Limit)
            .Select(request => _convert.FriendRequest(request, parameter.IsFiltered)));
    }
}

public class GetFriendRequestsParameter(int limit = 20, bool isFiltered = false)
{
    [JsonPropertyName("limit")]
    public int Limit { get; init; } = limit;

    [JsonPropertyName("is_filtered")]
    public bool IsFiltered { get; init; } = isFiltered;
}

public class GetFriendRequestsResult(IEnumerable<Entity.FriendRequest> requests)
{
    [JsonPropertyName("requests")]
    public IEnumerable<Entity.FriendRequest> Requests { get; } = requests;
}
