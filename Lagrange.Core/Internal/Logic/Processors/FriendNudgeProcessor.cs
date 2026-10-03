using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Packets.Notify;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Internal.Logic.Processors;

[MsgPushProcessor(MsgType.Event0x210, 290, true)]
internal sealed class FriendNudgeProcessor : MsgPushProcessorBase
{
    internal override ValueTask<bool> Handle(BotContext context, MsgType msgType, int subType,
        PushMessageEvent msgEvt, ReadOnlyMemory<byte>? content)
    {
        if (content is null) return ValueTask.FromResult(false);
        var tip = ProtoHelper.Deserialize<GeneralGrayTipInfo>(content.Value.Span);
        if (tip.BusiType != 12 || tip.MsgTemplParam is null) return ValueTask.FromResult(false);
        var values = tip.MsgTemplParam.ToDictionary(x => x.Name, x => x.Value);
        if (!values.TryGetValue("uin_str1", out var actor) || !values.TryGetValue("uin_str2", out var target)) return ValueTask.FromResult(false);
        var route = msgEvt.MsgPush.CommonMessage.RoutingHead;
        var from = route.FromUin != 0 ? route.FromUin : context.CacheContext.ResolveUin(route.FromUid);
        var to = route.ToUin != 0 ? route.ToUin : !string.IsNullOrWhiteSpace(route.ToUid) ? context.CacheContext.ResolveUin(route.ToUid) : context.BotUin;
        var peer = from == context.BotUin ? to : from;
        context.EventInvoker.PostEvent(new BotFriendNudgeEvent(
            peer,
            long.TryParse(actor, out var actorUin) ? actorUin : context.CacheContext.ResolveUin(actor),
            long.TryParse(target, out var targetUin) ? targetUin : context.CacheContext.ResolveUin(target),
            values.TryGetValue("action_str", out var action) ? action : values.GetValueOrDefault("alt_str1", string.Empty),
            values.GetValueOrDefault("action_img_url", string.Empty),
            values.GetValueOrDefault("suffix_str", string.Empty)));
        return ValueTask.FromResult(true);
    }
}
