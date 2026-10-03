using Lagrange.Core.Common.Response;
using Lagrange.Core.Events;

namespace Lagrange.Core.Internal.Events.System;

internal sealed class GetGroupHonorEventReq(long groupUin, string honorType) : ProtocolEvent { public long GroupUin { get; } = groupUin; public string HonorType { get; } = honorType; }
internal sealed class GetGroupHonorEventResp(BotGroupHonorResult result) : ProtocolEvent { public BotGroupHonorResult Result { get; } = result; }
