using Lagrange.Core.Common.Entity;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Notify;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Internal.Logic.Processors;

[MsgPushProcessor(MsgType.Event0x20D, true)]
[MsgPushProcessor(MsgType.GroupInvitation, true)]
internal class GroupInviteProcessor : MsgPushProcessorBase
{
    // another in `RichTextMsgProcessor` for private send invitation card.
    internal override async ValueTask<bool> Handle(BotContext context, MsgType msgType, int subType,
        PushMessageEvent msgEvt, ReadOnlyMemory<byte>? content)
    {
        GroupInviteBody body;
        if (msgType == MsgType.GroupInvitation)
        {
            var direct = ProtoHelper.Deserialize<DirectGroupInvite>(content!.Value.Span);
            body = new GroupInviteBody { GroupUin = direct.GroupUin, InviterUid = direct.InviterUid, TargetUid = context.Keystore.Uid };
        }
        else
        {
            var @event = ProtoHelper.Deserialize<Event0x20D>(content!.Value.Span);
            if (@event.SubType != 87) return false;
            body = ProtoHelper.Deserialize<GroupInvite>(@event.Body).Body;
        }
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(body.GroupUin);
        ArgumentException.ThrowIfNullOrWhiteSpace(body.InviterUid);
        ArgumentException.ThrowIfNullOrWhiteSpace(body.TargetUid);

        var response = await context.EventContext.SendEvent<FetchGroupNotificationsEventResp>(
            new FetchGroupNotificationsEventReq(100)
        );
        var inviteNotifications = response
            .GroupNotifications
            .OfType<BotGroupInviteNotification>();
        var matches = inviteNotifications.Where(notification =>
            body.GroupUin == notification.GroupUin &&
            body.InviterUid == notification.InviterUid &&
            body.TargetUid == notification.TargetUid &&
            notification.State == BotGroupNotificationState.Wait
        ).DistinctBy(x => (x.GroupUin, x.Sequence, x.IsFiltered)).ToArray();
        if (matches.Length > 1) throw new InvalidDataException("Multiple pending group invitations match this notice.");
        var notification = matches.SingleOrDefault();
        if (notification == null)
        {
            context.LogWarning(nameof(PushLogic),
                "Received GroupInviteNotification but no corresponding notification found");
            return false;
        }

        if (context.CacheContext.ObserveInvitation(notification)) context.EventInvoker.PostEvent(new BotGroupInviteNotificationEvent(notification));
        return true;
    }
}
