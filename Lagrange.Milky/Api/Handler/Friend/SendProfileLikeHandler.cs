using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handler.Friend;

[Api("send_profile_like")]
public class SendProfileLikeHandler(BotContext bot) : IEmptyResultApiHandler<SendProfileLikeParameter>
{
    private readonly BotContext _bot = bot;

    public Task HandleAsync(SendProfileLikeParameter parameter, CancellationToken token)
    {
        return _bot.SendProfileLike(parameter.UserId, (uint)parameter.Count);
    }
}

public class SendProfileLikeParameter(long userId, int count = 1)
{
    [JsonRequired]
    [JsonPropertyName("user_id")]
    public long UserId { get; init; } = userId;

    [JsonPropertyName("count")]
    public int Count { get; init; } = count;
}
