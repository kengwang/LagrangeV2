using System.Text.Json.Serialization;

namespace Lagrange.Milky.Models.Segments;

public class ImageIncomingSegment : IncomingSegmentBase<ImageIncomingSegmentData>;
public class ImageIncomingSegmentData
{
    [JsonPropertyName("resource_id")] public required string ResourceId { get; init; }
    [JsonPropertyName("temp_url")] public required string TempUrl { get; init; }
    [JsonPropertyName("width")] public required int Width { get; init; }
    [JsonPropertyName("height")] public required int Height { get; init; }
    [JsonPropertyName("summary")] public required string Summary { get; init; }
    [JsonPropertyName("sub_type")] public required string SubType { get; init; }
    [JsonPropertyName("file_md5")] public string? FileMd5 { get; init; }
    [JsonPropertyName("file_sha1")] public string? FileSha1 { get; init; }
    [JsonPropertyName("file_size")] public long FileSize { get; init; }
}

public sealed class ImageOutgoingSegment : OutgoingSegmentBase<ImageOutgoingSegmentData>;
public sealed class ImageOutgoingSegmentData(string uri, string? summary, string subType = "normal") {
    [JsonPropertyName("uri")] public required string Uri { get; init; } = uri;
    [JsonPropertyName("sub_type")] public string SubType { get; init; } = subType;
    [JsonPropertyName("summary")] public string? Summary { get; init; } = summary;
}
