using Lagrange.Core.Common.Response;
using Lagrange.Core.Events;

namespace Lagrange.Core.Internal.Events.System;

internal sealed class GetGroupEssenceEventReq(long groupUin, int pageStart, int pageLimit) : ProtocolEvent { public long GroupUin { get; } = groupUin; public int PageStart { get; } = pageStart; public int PageLimit { get; } = pageLimit; }
internal sealed class GetGroupEssenceEventResp(BotGroupEssenceResult result) : ProtocolEvent { public BotGroupEssenceResult Result { get; } = result; }
