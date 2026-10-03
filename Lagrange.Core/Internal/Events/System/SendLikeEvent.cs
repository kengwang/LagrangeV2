using Lagrange.Core.Events;
using Lagrange.Core.Common.Response;

namespace Lagrange.Core.Internal.Events.System;

internal class SendLikeEventReq(long userUin, uint count) : ProtocolEvent
{
    public long UserUin { get; } = userUin;
    public uint Count { get; } = count;
}

internal class SendLikeEventResp : ProtocolEvent
{
    public static readonly SendLikeEventResp Default = new();
}

internal class GetEmojiLikesEventReq(long groupUin, ulong sequence, string code, string cookie, uint count) : ProtocolEvent
{
    public long GroupUin { get; } = groupUin;
    public ulong Sequence { get; } = sequence;
    public string Code { get; } = code;
    public string Cookie { get; } = cookie;
    public uint Count { get; } = count;
}

internal class GetEmojiLikesEventResp(BotEmojiLikesResult result) : ProtocolEvent
{
    public BotEmojiLikesResult Result { get; } = result;
}

internal class GetGroupReactionSummaryEventReq(long groupUin, ulong sequence) : ProtocolEvent
{
    public long GroupUin { get; } = groupUin;
    public ulong Sequence { get; } = sequence;
}

internal class GetGroupReactionSummaryEventResp(BotGroupReactionSummaryResult result) : ProtocolEvent
{
    public BotGroupReactionSummaryResult Result { get; } = result;
}
