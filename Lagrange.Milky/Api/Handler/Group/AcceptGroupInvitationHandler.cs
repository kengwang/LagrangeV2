using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handler.Group;

[Api("accept_group_invitation")]
public class AcceptGroupInvitationHandler(BotContext bot) : IEmptyResultApiHandler<GroupInvitationOperateParameter>
{
    private readonly BotContext _bot = bot;

    public Task HandleAsync(GroupInvitationOperateParameter parameter, CancellationToken token)
    {
        return _bot.SetGroupNotification(parameter.GroupId, (ulong)parameter.InvitationSeq, BotGroupNotificationType.Invite, false, GroupNotificationOperate.Allow);
    }
}

public class GroupInvitationOperateParameter(long groupId, long invitationSeq)
{
    [JsonRequired]
    [JsonPropertyName("group_id")]
    public long GroupId { get; init; } = groupId;

    [JsonRequired]
    [JsonPropertyName("invitation_seq")]
    public long InvitationSeq { get; init; } = invitationSeq;
}
