using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Packets.Notify;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Internal.Logic.Processors;

[MsgPushProcessor(MsgType.GroupAdminChangedNotice, true)]
internal sealed class GroupAdminProcessor : MsgPushProcessorBase
{
    internal override ValueTask<bool> Handle(BotContext context, MsgType msgType, int subType,
        PushMessageEvent msgEvt, ReadOnlyMemory<byte>? content)
    {
        if (content is null) return ValueTask.FromResult(false);
        var admin = ProtoHelper.Deserialize<GroupAdmin>(content.Value.Span);
        var enable = admin.Body?.ExtraEnable?.AdminUid;
        var disable = admin.Body?.ExtraDisable?.AdminUid;
        var uid = !string.IsNullOrWhiteSpace(enable) ? enable : disable;
        if (string.IsNullOrWhiteSpace(uid) || admin.GroupUin == 0) return ValueTask.FromResult(false);
        context.EventInvoker.PostEvent(new BotGroupAdminEvent(admin.GroupUin, context.CacheContext.ResolveUin(uid), !string.IsNullOrWhiteSpace(enable)));
        return ValueTask.FromResult(true);
    }
}
