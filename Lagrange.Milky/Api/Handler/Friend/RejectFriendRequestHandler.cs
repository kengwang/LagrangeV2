using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handler.Friend;

[Api("reject_friend_request")]
public class RejectFriendRequestHandler(BotContext bot) : IEmptyResultApiHandler<RejectFriendRequestParameter>
{
    private readonly BotContext _bot = bot;

    public Task HandleAsync(RejectFriendRequestParameter parameter, CancellationToken token)
    {
        return _bot.SetFriendRequest(parameter.InitiatorUid, false);
    }
}

public class RejectFriendRequestParameter(string initiatorUid, bool isFiltered = false, string? reason = null)
{
    [JsonRequired]
    [JsonPropertyName("initiator_uid")]
    public string InitiatorUid { get; init; } = initiatorUid;

    [JsonPropertyName("is_filtered")]
    public bool IsFiltered { get; init; } = isFiltered;

    [JsonPropertyName("reason")]
    public string? Reason { get; init; } = reason;
}
