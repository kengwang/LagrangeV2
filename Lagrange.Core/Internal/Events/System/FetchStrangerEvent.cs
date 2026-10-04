using Lagrange.Core.Common.Entity;
using Lagrange.Core.Events;
using Lagrange.Core.Internal.Packets.Service.Migration;

namespace Lagrange.Core.Internal.Events.System;

internal abstract class FetchStrangerEventReqBase : ProtocolEvent { }

internal class FetchStrangerByUinEventReq(long uin) : FetchStrangerEventReqBase
{
    public long Uin { get; } = uin;
}

internal class FetchStrangerByUidEventReq(string uid) : FetchStrangerEventReqBase
{
    public string Uid { get; } = uid;
}

internal class FetchStrangerEventResp(Func<BotStranger> stranger, OidbStrangerStatusResp body) : GetUserStatusEventResp(body)
{
    private readonly Lazy<BotStranger> _stranger = new(stranger);
    public BotStranger Stranger => _stranger.Value;
}
