using System.Text.RegularExpressions;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Packets.Notify;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Internal.Logic.Processors;

[MsgPushProcessor(MsgType.Event0x210, 39, true)]
internal sealed class FriendProfileLikeProcessor : MsgPushProcessorBase
{
    internal override ValueTask<bool> Handle(BotContext context, MsgType msgType, int subType,
        PushMessageEvent msgEvt, ReadOnlyMemory<byte>? content)
    {
        if (content is null) return ValueTask.FromResult(false);
        var tip = ProtoHelper.Deserialize<ProfileLikeTip>(content.Value.Span);
        if (tip.MsgType != 0 || tip.SubType != 203 || tip.Content?.Msg?.Detail is not { } detail || detail.Uin == 0)
            return ValueTask.FromResult(false);

        var times = detail.Text is null
            ? tip.Content.Msg.Times
            : int.TryParse(Regex.Match(detail.Text, "\\d+").Value, out var parsed) ? parsed : tip.Content.Msg.Times;
        context.EventInvoker.PostEvent(new BotFriendProfileLikeEvent(detail.Uin, detail.Nickname ?? string.Empty, times));
        return ValueTask.FromResult(true);
    }
}
