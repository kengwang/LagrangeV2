using System.Text.Json.Serialization;
namespace Lagrange.Milky.Models.Segments;
public sealed class FlashFileIncomingSegment : IncomingSegmentBase<FlashFileIncomingSegmentData>;
public sealed class FlashFileIncomingSegmentData
{
    [JsonPropertyName("fileset_id")] public required string FilesetId { get; init; }
    [JsonPropertyName("file_name")] public required string FileName { get; init; }
    [JsonPropertyName("thumb_url")] public string? ThumbnailUrl { get; init; }
    [JsonPropertyName("scene_type")] public uint SceneType { get; init; }
}
