using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handler.Group;

[Api("set_group_member_mute")]
public class SetGroupMemberMuteHandler(BotContext bot) : IEmptyResultApiHandler<SetGroupMemberMuteParameter>
{
    private readonly BotContext _bot = bot;

    public Task HandleAsync(SetGroupMemberMuteParameter parameter, CancellationToken token)
    {
        return _bot.GroupMuteMember(parameter.GroupId, parameter.UserId, (uint)parameter.Duration);
    }
}

public class SetGroupMemberMuteParameter(long groupId, long userId, int duration = 0)
{
    [JsonRequired]
    [JsonPropertyName("group_id")]
    public long GroupId { get; init; } = groupId;

    [JsonRequired]
    [JsonPropertyName("user_id")]
    public long UserId { get; init; } = userId;

    [JsonPropertyName("duration")]
    public int Duration { get; init; } = duration;
}
