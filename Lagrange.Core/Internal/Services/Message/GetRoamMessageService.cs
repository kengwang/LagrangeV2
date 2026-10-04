using Lagrange.Core.Common;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Services;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Internal.Services.Message;

[EventSubscribe<GetRoamMessageEventReq>(Protocols.All)]
[Service("trpc.msg.register_proxy.RegisterProxy.SsoGetRoamMsg")]
internal class GetRoamMessageService : BaseService<GetRoamMessageEventReq, GetRoamMessageEventResp>
{
    protected override ValueTask<ReadOnlyMemory<byte>> Build(GetRoamMessageEventReq input, BotContext context)
    {
        var packet = new SsoGetRoamMsgReq
        {
            PeerUid = input.PeerUid,
            Time = input.Time,
            Random = input.Random,
            Count = input.Count,
            Direction = input.Direction
        };

        return ValueTask.FromResult(ProtoHelper.Serialize(packet));
    }

    protected override ValueTask<GetRoamMessageEventResp> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        if (input.IsEmpty) throw new OperationException(-1, "Private roam history returned an empty protocol response.");
        var packet = ProtoHelper.Deserialize<SsoGetRoamMsgRsp>(input.Span);
        return ValueTask.FromResult(new GetRoamMessageEventResp(packet.Messages, packet.Timestamp, packet.Random, packet.IsComplete, packet.PeerUid));
    }
}
