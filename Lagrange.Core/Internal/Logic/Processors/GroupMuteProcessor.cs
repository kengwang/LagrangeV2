using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Packets.Notify;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Internal.Logic.Processors;

[MsgPushProcessor(MsgType.Event0x2DC, 12, true)]
internal sealed class GroupMuteProcessor : MsgPushProcessorBase
{
    internal override ValueTask<bool> Handle(BotContext context, MsgType msgType, int subType,
        PushMessageEvent msgEvt, ReadOnlyMemory<byte>? content)
    {
        if (content is null) return ValueTask.FromResult(false);
        var mute = ProtoHelper.Deserialize<GroupMute>(content.Value.Span);
        if (mute.Data?.State is null || mute.GroupUin == 0) return ValueTask.FromResult(false);
        var operatorUin = string.IsNullOrWhiteSpace(mute.OperatorUid) ? 0 : context.CacheContext.ResolveUin(mute.OperatorUid);
        var userUin = string.IsNullOrWhiteSpace(mute.Data.State.TargetUid) ? 0 : context.CacheContext.ResolveUin(mute.Data.State.TargetUid);
        var duration = mute.Data.State.Duration.GetValueOrDefault() == uint.MaxValue ? uint.MaxValue : mute.Data.State.Duration.GetValueOrDefault();
        context.EventInvoker.PostEvent(userUin == 0
            ? new BotGroupWholeMuteEvent(mute.GroupUin, operatorUin, duration)
            : new BotGroupMuteEvent(mute.GroupUin, operatorUin, userUin, duration));
        return ValueTask.FromResult(true);
    }
}
