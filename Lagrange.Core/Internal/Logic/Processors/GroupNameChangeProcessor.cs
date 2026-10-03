using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Packets.Notify;
using Lagrange.Core.Utility;
using Lagrange.Core.Utility.Binary;

namespace Lagrange.Core.Internal.Logic.Processors;

[MsgPushProcessor(MsgType.Event0x2DC, 16, true)]
internal sealed class GroupNameChangeProcessor : MsgPushProcessorBase
{
    internal override ValueTask<bool> Handle(BotContext context, MsgType msgType, int subType,
        PushMessageEvent msgEvt, ReadOnlyMemory<byte>? content)
    {
        if (content is null || content.Value.Length <= 5) return ValueTask.FromResult(false);
        var packet = new BinaryPacket(content.Value.Span);
        packet.Skip(5);
        var notify = ProtoHelper.Deserialize<NotifyMessageBody>(packet.ReadBytes(Prefix.Int16 | Prefix.LengthOnly));
        if (notify.SubType != 12 || notify.EventParam is null) return ValueTask.FromResult(false);
        var name = ProtoHelper.Deserialize<GroupNameChange>(notify.EventParam).Name;
        if (string.IsNullOrWhiteSpace(name)) return ValueTask.FromResult(false);
        var operatorUin = string.IsNullOrWhiteSpace(notify.OperatorUid)
            ? 0
            : context.CacheContext.ResolveUin(notify.OperatorUid);
        context.EventInvoker.PostEvent(new BotGroupNameChangeEvent(notify.GroupUin, name, operatorUin));
        return ValueTask.FromResult(true);
    }
}
