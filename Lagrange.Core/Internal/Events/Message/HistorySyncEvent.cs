using Lagrange.Core.Events;
using Lagrange.Core.Internal.Packets.Message;
namespace Lagrange.Core.Internal.Events.Message;
internal sealed class HistorySyncProbeEventReq(SsoReadedReportReq body) : ProtocolEvent
{
    public SsoReadedReportReq Body { get; } = body;
}
