using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handler.Friend;

[Api("delete_friend")]
public class DeleteFriendHandler(BotContext bot) : IEmptyResultApiHandler<DeleteFriendParameter>
{
    private readonly BotContext _bot = bot;

    public Task HandleAsync(DeleteFriendParameter parameter, CancellationToken token)
    {
        return _bot.DeleteFriend(parameter.UserId);
    }
}

public class DeleteFriendParameter(long userId)
{
    [JsonRequired]
    [JsonPropertyName("user_id")]
    public long UserId { get; init; } = userId;
}
