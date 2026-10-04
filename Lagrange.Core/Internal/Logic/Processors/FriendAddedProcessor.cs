using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Packets.Notify;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Internal.Logic.Processors;

[MsgPushProcessor(MsgType.Event0x210, 179, true)]
[MsgPushProcessor(MsgType.Event0x210, 226, true)]
internal sealed class FriendAddedProcessor : MsgPushProcessorBase
{
    internal override ValueTask<bool> Handle(BotContext context, MsgType msgType, int subType, PushMessageEvent msgEvt, ReadOnlyMemory<byte>? content)
    {
        if (content is null) return ValueTask.FromResult(false);
        var friend = ProtoHelper.Deserialize<NewFriend>(content.Value.Span);
        if (friend.Info is null || string.IsNullOrWhiteSpace(friend.Info.Uid)) return ValueTask.FromResult(false);
        context.EventInvoker.PostEvent(new BotFriendAddedEvent(
            context.CacheContext.ResolveUin(friend.Info.Uid), friend.Info.Uid,
            friend.Info.Nickname ?? string.Empty, friend.Info.Time));
        return ValueTask.FromResult(true);
    }
}
