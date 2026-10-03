using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Milky.Events.Attributes;
namespace Lagrange.Milky.Events.Converters;
[EventConverter]
public sealed class PeerPinChangeEventConverter : IEventConverter<BotPeerPinChangeEvent, PeerPinChangeEventConverter.Data>
{
    public string Name => "peer_pin_change";
    public bool CanConvert(BotPeerPinChangeEvent @event) => true;
    public ValueTask<Data> ConvertAsync(BotPeerPinChangeEvent @event, CancellationToken ct) => ValueTask.FromResult(new Data { PeerType = @event.Scene, PeerId = @event.PeerId, IsPinned = @event.IsPinned });
    public sealed class Data { [JsonPropertyName("peer_type")] public required string PeerType { get; init; } [JsonPropertyName("peer_id")] public long PeerId { get; init; } [JsonPropertyName("is_pinned")] public bool IsPinned { get; init; } }
}
