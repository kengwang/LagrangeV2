using Lagrange.Core.Common.Response;
using Lagrange.Core.Events;

namespace Lagrange.Core.Internal.Events.System;

internal sealed class GetQzoneFeedsEventReq(long userUin, int page, int count) : ProtocolEvent
{
    public long UserUin { get; } = userUin;
    public int Page { get; } = page;
    public int Count { get; } = count;
}
internal sealed class GetQzoneFeedsEventResp(BotQzoneFeedResult result) : ProtocolEvent { public BotQzoneFeedResult Result { get; } = result; }

internal sealed class GetQzoneMessageListEventReq(long userUin, int position, int count) : ProtocolEvent
{
    public long UserUin { get; } = userUin;
    public int Position { get; } = position;
    public int Count { get; } = count;
}
internal sealed class GetQzoneMessageListEventResp(BotQzoneMessageListResult result) : ProtocolEvent { public BotQzoneMessageListResult Result { get; } = result; }
