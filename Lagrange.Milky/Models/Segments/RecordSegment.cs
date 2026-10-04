using System.Text.Json.Serialization;

namespace Lagrange.Milky.Models.Segments;

public class RecordIncomingSegment : IncomingSegmentBase<RecordIncomingSegmentData>;
public class RecordIncomingSegmentData
{
    [JsonPropertyName("resource_id")] public required string ResourceId { get; init; }
    [JsonPropertyName("temp_url")] public required string TempUrl { get; init; }
    [JsonPropertyName("duration")] public required int Duration { get; init; }
    [JsonPropertyName("file_md5")] public string? FileMd5 { get; init; }
    [JsonPropertyName("file_sha1")] public string? FileSha1 { get; init; }
    [JsonPropertyName("file_size")] public long FileSize { get; init; }
}

public sealed class RecordOutgoingSegment : OutgoingSegmentBase<RecordOutgoingSegmentData>;
public sealed class RecordOutgoingSegmentData(string uri) {
    [JsonPropertyName("uri")] public required string Uri { get; init; } = uri;
}
