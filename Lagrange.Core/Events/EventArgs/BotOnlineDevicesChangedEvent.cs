namespace Lagrange.Core.Events.EventArgs;

public sealed record BotOnlineDevice(uint AppId, uint InstanceId, uint ClientType, uint Platform, string DeviceName);

public sealed class BotOnlineDevicesChangedEvent(IReadOnlyList<BotOnlineDevice> devices) : EventBase
{
    public IReadOnlyList<BotOnlineDevice> Devices { get; } = devices;

    public override string ToEventMessage() => $"{nameof(BotOnlineDevicesChangedEvent)}: Devices={Devices.Count}";
}
