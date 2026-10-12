using System.Text.Json;
using System.Web;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entities;

namespace Lagrange.Core.Internal.Logic.Processors;

[MsgPushProcessor(MsgType.GroupMessage)]
[MsgPushProcessor(MsgType.PrivateMessage)]
[MsgPushProcessor(MsgType.PrivateFileMessage)]
[MsgPushProcessor(MsgType.TempMessage)]
internal class RichTextMsgProcessor : MsgPushProcessorBase
{
    internal override async ValueTask<bool> Handle(BotContext context, MsgType msgType, int subType, PushMessageEvent msgEvt, ReadOnlyMemory<byte>? content)
    {
        var head = msgEvt.MsgPush.CommonMessage.RoutingHead;
        if (msgType == MsgType.GroupMessage && head.Group is { } group &&
            context.CacheContext.ObserveCard(group.GroupCode, head.FromUin, group.GroupCard) is { } changed)
            context.EventInvoker.PostEvent(changed);

        var message = await context.EventContext.GetLogic<MessagingLogic>().Parse(msgEvt.MsgPush.CommonMessage);

        if (message.Contact is BotGroupMember groupMember && message.Entities.OfType<GroupFileEntity>().FirstOrDefault() is { } uploadedFile)
        {
            context.EventInvoker.PostEvent(new BotGroupFileUploadEvent(
                groupMember.Group.GroupUin,
                message.Contact.Uin,
                uploadedFile.FileId,
                uploadedFile.FileName,
                uploadedFile.FileSize));
        }
        else if (message.Contact is BotFriend friend && message.Entities.OfType<GroupFileEntity>().FirstOrDefault() is { } privateFile)
        {
            context.EventInvoker.PostEvent(new BotFriendFileUploadEvent(
                friend.Uin, message.Contact.Uin, privateFile.FileId, privateFile.FileName, privateFile.FileSize));
        }

        if (message.Entities.Count > 0 && message.Entities[0] is LightAppEntity app && TryHandleLightApp(context, message, app)) return true;

        context.VoiceContext.OnMessage(message);
        context.EventInvoker.PostEvent(new BotMessageEvent(message, msgEvt.Raw));
        return true;
    }

    private bool TryHandleLightApp(BotContext context, BotMessage message, LightAppEntity app)
    {
        if ((app.AppName is "com.tencent.qun.invite" or "com.tencent.tuwen.lua") && TryHandleQunInvite(context, message, app)) return true;

        return false;
    }

    private bool TryHandleQunInvite(BotContext context, BotMessage message, LightAppEntity app)
    {
        using var document = JsonDocument.Parse(app.Payload);
        var root = document.RootElement;

        if (root.ValueKind != JsonValueKind.Object) return false;
        if (app.AppName == "com.tencent.qun.invite" &&
            (!root.TryGetProperty("bizsrc", out var bizsrc) ||
             bizsrc.ValueKind != JsonValueKind.String || bizsrc.GetString() != "qun.invite")) return false;

        // com.tencent.tuwen.lua is also used by ordinary shares, such as QQ Music.
        // Only consume the card when it contains valid group invitation parameters.
        if (!root.TryGetProperty("meta", out var meta) || meta.ValueKind != JsonValueKind.Object ||
            !meta.TryGetProperty("news", out var news) || news.ValueKind != JsonValueKind.Object ||
            !news.TryGetProperty("jumpUrl", out var jumpUrl) || jumpUrl.ValueKind != JsonValueKind.String ||
            !Uri.TryCreate(jumpUrl.GetString(), UriKind.Absolute, out var url)) return false;

        var query = HttpUtility.ParseQueryString(url.Query);
        if (!uint.TryParse(query["groupcode"], out var uin) ||
            !ulong.TryParse(query["msgseq"], out var sequence)) return false;

        context.EventInvoker.PostEvent(new BotGroupInviteNotificationEvent(new BotGroupInviteNotification(
            uin,
            sequence,
            context.BotUin,
            context.CacheContext.ResolveCachedUid(context.BotUin) ?? string.Empty,
            BotGroupNotificationState.Wait,
            null,
            null,
            message.Contact.Uin,
            message.Contact.Uid,
            false
        )));

        return true;
    }
}
