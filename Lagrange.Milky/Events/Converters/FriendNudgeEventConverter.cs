using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Milky.Events.Attributes;

namespace Lagrange.Milky.Events.Converters;

[EventConverter]
public sealed class FriendNudgeEventConverter : IEventConverter<BotFriendNudgeEvent, FriendNudgeEventConverter.Data>
{
    public string Name => "friend_nudge";
    public bool CanConvert(BotFriendNudgeEvent @event) => true;
    public ValueTask<Data> ConvertAsync(BotFriendNudgeEvent @event, CancellationToken ct) => ValueTask.FromResult(new Data
    {
        PeerId = @event.PeerUin,
        SenderId = @event.SenderUin,
        ReceiverId = @event.TargetUin,
        DisplayAction = @event.Action,
        DisplayActionImgUrl = @event.ActionImageUrl,
        DisplaySuffix = @event.Suffix,
    });

    public sealed class Data
    {
        [JsonPropertyName("peer_id")] public required long PeerId { get; init; }
        [JsonPropertyName("sender_id")] public required long SenderId { get; init; }
        [JsonPropertyName("receiver_id")] public required long ReceiverId { get; init; }
        [JsonPropertyName("display_action")] public required string DisplayAction { get; init; }
        [JsonPropertyName("display_action_img_url")] public required string DisplayActionImgUrl { get; init; }
        [JsonPropertyName("display_suffix")] public required string DisplaySuffix { get; init; }
    }
}
