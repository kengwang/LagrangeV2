using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Milky.Events.Attributes;

namespace Lagrange.Milky.Events.Converters;

[EventConverter]
public sealed class GroupFileUploadEventConverter : IEventConverter<BotGroupFileUploadEvent, GroupFileUploadEventConverter.Data>
{
    public string Name => "group_file_upload";
    public bool CanConvert(BotGroupFileUploadEvent @event) => true;
    public ValueTask<Data> ConvertAsync(BotGroupFileUploadEvent @event, CancellationToken ct) => ValueTask.FromResult(new Data
    {
        GroupId = @event.GroupUin,
        UserId = @event.UserUin,
        FileId = @event.FileId,
        FileName = @event.FileName,
        FileSize = @event.FileSize,
        BusId = @event.BusId,
    });

    public sealed class Data
    {
        [JsonPropertyName("group_id")] public required long GroupId { get; init; }
        [JsonPropertyName("user_id")] public required long UserId { get; init; }
        [JsonPropertyName("file_id")] public required string FileId { get; init; }
        [JsonPropertyName("file_name")] public required string FileName { get; init; }
        [JsonPropertyName("file_size")] public required long FileSize { get; init; }
        [JsonPropertyName("bus_id")] public required long BusId { get; init; }
    }
}
