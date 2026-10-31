using Lagrange.Core.Common;
using Lagrange.Core.Events;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Message;
using Lagrange.Core.Services;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Internal.Services.Message;

[EventSubscribe<SendMessageEventReq>(Protocols.All)]
[EventSubscribe<SendFriendFileEventReq>(Protocols.All)]
[Service("MessageSvc.PbSendMsg")]
internal class SendMessageService : BaseService<ProtocolEvent, ProtocolEvent>
{
    protected override ValueTask<ReadOnlyMemory<byte>> Build(ProtocolEvent input, BotContext context)
    {
        switch (input)
        {
            case SendMessageEventReq sendMsg:
            {
                var payload = MessagePacker.Build(sendMsg.Message);
                return new ValueTask<ReadOnlyMemory<byte>>(payload);
            }
            case SendFriendFileEventReq file:
            {
                var payload = MessagePacker.BuildTrans0X211(file.Friend, file.Request, file.Response, file.ClientSequence, file.Sequence);
                return new ValueTask<ReadOnlyMemory<byte>>(payload);
            }
        }
        
        throw new NotSupportedException($"Unsupported input type: {input.GetType()}");
    }

    protected override ValueTask<ProtocolEvent> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        if (input.IsEmpty) throw new InvalidDataException("Message send confirmation is missing.");
        var response = ProtoHelper.Deserialize<PbSendMsgResp>(input.Span);
        return ValueTask.FromResult<ProtocolEvent>(ReadConfirmation(response));
    }

    internal static SendMessageEventResp ReadConfirmation(PbSendMsgResp response)
    {
        var code = response.Result != 0 ? response.Result : response.ReviewResult != 0 ? response.ReviewResult : response.ServerResult?.Code ?? 0;
        if (code != 0) return new SendMessageEventResp(code, response.SendTime, 0, response.Result != 0 || response.ReviewResult != 0 ? response.ErrMsg : response.ServerResult?.Message);
        ulong sequence = response.ClientSequence == 0 ? response.Sequence : response.ClientSequence;
        if (sequence == 0 || response.SendTime <= 0) throw new InvalidDataException("Message send response has no authoritative sequence or timestamp.");
        return new SendMessageEventResp(0, response.SendTime, sequence, response.ErrMsg);
    }
}
