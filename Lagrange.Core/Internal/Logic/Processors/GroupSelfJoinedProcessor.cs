using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Packets.Notify;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Internal.Logic.Processors;

[MsgPushProcessor(MsgType.GroupSelfJoinedNotice, true)]
internal sealed class GroupSelfJoinedProcessor : MsgPushProcessorBase
{
    internal override ValueTask<bool> Handle(BotContext context, MsgType msgType, int subType, PushMessageEvent msgEvt, ReadOnlyMemory<byte>? content)
    {
        if (content is null) return ValueTask.FromResult(false);
        var joined = ProtoHelper.Deserialize<SelfJoinInGroup>(content.Value.Span);
        if (joined.GroupUin == 0) return ValueTask.FromResult(false);
        var operatorUin = string.IsNullOrWhiteSpace(joined.OperatorUid) ? 0 : context.CacheContext.ResolveUin(joined.OperatorUid);
        context.EventInvoker.PostEvent(new BotGroupSelfJoinedEvent(joined.GroupUin, operatorUin, joined.OperatorUid ?? string.Empty));
        return ValueTask.FromResult(true);
    }
}
