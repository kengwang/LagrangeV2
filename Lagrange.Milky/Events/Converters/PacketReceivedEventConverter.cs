using Lagrange.Core.Events.EventArgs;
using Lagrange.Milky.Models;

namespace Lagrange.Milky.Events.Converters;

/// <summary>Converts raw SSO events. Registered manually so subscriptions remain opt-in.</summary>
public sealed class PacketReceivedEventConverter : IEventConverter<BotPacketReceivedEvent, RawPacketData>
{
    /// <inheritdoc />
    public string Name => "packet_received";
    /// <inheritdoc />
    public bool CanConvert(BotPacketReceivedEvent @event) => true;
    /// <inheritdoc />
    public ValueTask<RawPacketData> ConvertAsync(BotPacketReceivedEvent @event, CancellationToken ct) =>
        ValueTask.FromResult(RawPacketData.FromEvent(@event));
}
