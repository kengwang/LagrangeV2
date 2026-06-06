using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handler.Group;

[Api("kick_group_member")]
public class KickGroupMemberHandler(BotContext bot) : IEmptyResultApiHandler<KickGroupMemberParameter>
{
    private readonly BotContext _bot = bot;

    public Task HandleAsync(KickGroupMemberParameter parameter, CancellationToken token)
    {
        return _bot.GroupKickMember(parameter.GroupId, parameter.UserId, parameter.RejectAddRequest);
    }
}

public class KickGroupMemberParameter(long groupId, long userId, bool rejectAddRequest = false)
{
    [JsonRequired]
    [JsonPropertyName("group_id")]
    public long GroupId { get; init; } = groupId;

    [JsonRequired]
    [JsonPropertyName("user_id")]
    public long UserId { get; init; } = userId;

    [JsonPropertyName("reject_add_request")]
    public bool RejectAddRequest { get; init; } = rejectAddRequest;
}
