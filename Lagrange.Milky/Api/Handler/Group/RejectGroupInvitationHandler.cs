using Lagrange.Core;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handler.Group;

[Api("reject_group_invitation")]
public class RejectGroupInvitationHandler(BotContext bot) : IEmptyResultApiHandler<GroupInvitationOperateParameter>
{
    private readonly BotContext _bot = bot;

    public Task HandleAsync(GroupInvitationOperateParameter parameter, CancellationToken token)
    {
        return _bot.SetGroupNotification(parameter.GroupId, (ulong)parameter.InvitationSeq, BotGroupNotificationType.Invite, false, GroupNotificationOperate.Deny);
    }
}
