using Lagrange.Core.Common;
using Lagrange.Core.Internal.Events;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Internal.Services.Message;

[EventSubscribe<MarkReadEventReq>(Protocols.All)]
[Service("trpc.msg.msg_svc.MsgService.SsoReadedReport")]
internal class MarkReadService : BaseService<MarkReadEventReq, MarkReadEventResp>
{
    protected override ValueTask<ReadOnlyMemory<byte>> Build(MarkReadEventReq input, BotContext context)
    {
        var packet = input.TargetUid == null ? new SsoReadedReport
        {
            Group = new SsoReadedReportGroup
            {
                GroupUin = input.GroupUin,
                StartSequence = input.StartSequence
            }
        } : new SsoReadedReport
        {
            C2C = new SsoReadedReportC2C
            {
                TargetUid = input.TargetUid,
                Time = input.Time,
                StartSequence = input.StartSequence
            }
        };

        return ValueTask.FromResult(ProtoHelper.Serialize(packet));
    }

    protected override ValueTask<MarkReadEventResp> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        return ValueTask.FromResult(MarkReadEventResp.Default);
    }
}
