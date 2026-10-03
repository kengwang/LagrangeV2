using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Milky.Events.Attributes;

namespace Lagrange.Milky.Events.Converters;

[EventConverter]
public sealed class FriendFileUploadEventConverter : IEventConverter<BotFriendFileUploadEvent, FriendFileUploadEventConverter.Data>
{
    public string Name => "friend_file_upload";
    public bool CanConvert(BotFriendFileUploadEvent @event) => true;
    public ValueTask<Data> ConvertAsync(BotFriendFileUploadEvent @event, CancellationToken ct) => ValueTask.FromResult(new Data
    {
        PeerId = @event.PeerUin, UserId = @event.UserUin, FileId = @event.FileId, FileName = @event.FileName, FileSize = @event.FileSize,
    });

    public sealed class Data
    {
        [JsonPropertyName("peer_id")] public required long PeerId { get; init; }
        [JsonPropertyName("user_id")] public required long UserId { get; init; }
        [JsonPropertyName("file_id")] public required string FileId { get; init; }
        [JsonPropertyName("file_name")] public required string FileName { get; init; }
        [JsonPropertyName("file_size")] public required long FileSize { get; init; }
    }
}
