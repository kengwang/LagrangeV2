using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handler.Friend;

[Api("accept_friend_request")]
public class AcceptFriendRequestHandler(BotContext bot) : IEmptyResultApiHandler<AcceptFriendRequestParameter>
{
    private readonly BotContext _bot = bot;

    public Task HandleAsync(AcceptFriendRequestParameter parameter, CancellationToken token)
    {
        return _bot.SetFriendRequest(parameter.InitiatorUid, true);
    }
}

public class AcceptFriendRequestParameter(string initiatorUid, bool isFiltered = false)
{
    [JsonRequired]
    [JsonPropertyName("initiator_uid")]
    public string InitiatorUid { get; init; } = initiatorUid;

    [JsonPropertyName("is_filtered")]
    public bool IsFiltered { get; init; } = isFiltered;
}
