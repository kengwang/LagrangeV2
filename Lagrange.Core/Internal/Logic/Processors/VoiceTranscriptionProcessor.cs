using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Packets.Service.Migration;
using Lagrange.Core.Utility;
namespace Lagrange.Core.Internal.Logic.Processors;
[MsgPushProcessor(MsgType.Event0x210, 61, true)]
internal sealed class VoiceTranscriptionProcessor : MsgPushProcessorBase
{
    internal override ValueTask<bool> Handle(BotContext context, MsgType msgType, int subType, PushMessageEvent msgEvt, ReadOnlyMemory<byte>? content)
    {
        if (content is null) return ValueTask.FromResult(false);
        var item = ProtoHelper.Deserialize<PttTransPush>(content.Value.Span).Item;
        if (item is null || item.MsgId == 0) return ValueTask.FromResult(false);
        context.VoiceContext.CompleteTranscription(item.MsgId, item.Text, item.SenderUin, item.Uuid);
        return ValueTask.FromResult(true);
    }
}
