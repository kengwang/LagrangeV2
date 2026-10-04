using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Packets.Notify;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Internal.Logic.Processors;

/// <summary>Additional 0x210 notifications surfaced by QQ NT.</summary>
[MsgPushProcessor(MsgType.Event0x210, 277, true)]
[MsgPushProcessor(MsgType.Event0x210, 364, true)]
[MsgPushProcessor(MsgType.Event0x210, 349, true)]
internal sealed class FriendExtendedEventProcessor : MsgPushProcessorBase
{
    internal override async ValueTask<bool> Handle(BotContext context, MsgType msgType, int subType, PushMessageEvent msgEvt, ReadOnlyMemory<byte>? content)
    {
        if (content is null) return false;

        switch (subType)
        {
            case 277:
            {
                var notify = ProtoHelper.Deserialize<InputStatusNotify>(content.Value.Span);
                if (string.IsNullOrWhiteSpace(notify.FromUid)) return false;
                context.EventInvoker.PostEvent(new BotFriendInputStatusEvent(
                    context.CacheContext.ResolveUin(notify.FromUid), notify.FromUid, notify.NotifyItem?.EventType ?? 1));
                return true;
            }
            case 364:
            {
                var notify = ProtoHelper.Deserialize<FriendRemarkChangedNotify>(content.Value.Span);
                var target = notify.Change?.Target;
                if (target is null || (target.Uin == 0 && string.IsNullOrWhiteSpace(target.Uid))) return false;
                var uin = target.Uin != 0 ? target.Uin : context.CacheContext.ResolveUin(target.Uid);
                await context.CacheContext.UpdateRemark(uin, target.Uid ?? string.Empty, notify.Change?.Remark ?? string.Empty);
                context.EventInvoker.PostEvent(new BotFriendRemarkChangedEvent(uin, target.Uid ?? string.Empty, notify.Change?.Remark ?? string.Empty));
                return true;
            }
            case 349:
            {
                var notify = ProtoHelper.Deserialize<OnlineDeviceNotify>(content.Value.Span);
                var devices = notify.Devices.Select(x => new BotOnlineDevice(
                    x.AppId, x.InstanceId, x.ClientType & 0xff, x.Platform, x.DeviceName ?? string.Empty)).ToArray();
                context.CacheContext.SetOnlineDevices(devices);
                context.EventInvoker.PostEvent(new BotOnlineDevicesChangedEvent(devices));
                return true;
            }
            default:
                return false;
        }
    }
}
