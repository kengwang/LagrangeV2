using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handler.Group;

[Api("set_group_member_admin")]
public class SetGroupMemberAdminHandler(BotContext bot) : IEmptyResultApiHandler<SetGroupMemberAdminParameter>
{
    private readonly BotContext _bot = bot;

    public Task HandleAsync(SetGroupMemberAdminParameter parameter, CancellationToken token)
    {
        return _bot.GroupSetAdmin(parameter.GroupId, parameter.UserId, parameter.IsSet);
    }
}

public class SetGroupMemberAdminParameter(long groupId, long userId, bool isSet = true)
{
    [JsonRequired]
    [JsonPropertyName("group_id")]
    public long GroupId { get; init; } = groupId;

    [JsonRequired]
    [JsonPropertyName("user_id")]
    public long UserId { get; init; } = userId;

    [JsonPropertyName("is_set")]
    public bool IsSet { get; init; } = isSet;
}
