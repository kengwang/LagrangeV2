using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Packets.Notify;
using Lagrange.Core.Utility;
using Lagrange.Core.Utility.Binary;

namespace Lagrange.Core.Internal.Logic.Processors;

[MsgPushProcessor(MsgType.Event0x2DC, 21, true)]
internal sealed class GroupEssenceProcessor : MsgPushProcessorBase
{
    internal override ValueTask<bool> Handle(BotContext context, MsgType msgType, int subType,
        PushMessageEvent msgEvt, ReadOnlyMemory<byte>? content)
    {
        if (content is null || content.Value.Length < 7) return ValueTask.FromResult(false);
        var packet = new BinaryPacket(content.Value.Span);
        packet.Skip(5);
        var notify = ProtoHelper.Deserialize<NotifyMessageBody>(packet.ReadBytes(Prefix.Int16 | Prefix.LengthOnly));
        var essence = notify.EssenceMessage;
        if (essence is null) return ValueTask.FromResult(false);
        var isSet = (essence.SetFlag != 0 ? essence.SetFlag : essence.SetFlag2) == 1;
        context.EventInvoker.PostEvent(new BotGroupEssenceEvent(
            essence.GroupUin != 0 ? essence.GroupUin : notify.GroupUin,
            essence.MsgSequence != 0 ? essence.MsgSequence : essence.MsgSequence2 != 0 ? essence.MsgSequence2 : notify.MsgSequence,
            essence.Random,
            essence.MemberUin,
            essence.OperatorUin,
            isSet,
            essence.TimeStamp));
        return ValueTask.FromResult(true);
    }
}
