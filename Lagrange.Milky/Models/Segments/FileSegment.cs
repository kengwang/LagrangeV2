using System.Text.Json.Serialization;

namespace Lagrange.Milky.Models.Segments;

public class FileIncomingSegment : IncomingSegmentBase<FileIncomingSegmentData>;
public class FileIncomingSegmentData
{
    [JsonPropertyName("file_id")] public required string FileId { get; init; }
    [JsonPropertyName("file_name")] public required string FileName { get; init; }
    [JsonPropertyName("file_size")] public required long FileSize { get; init; }
    [JsonPropertyName("file_hash")] public string? FileHash { get; init; }
    [JsonPropertyName("url")] public string? Url { get; init; }
    [JsonPropertyName("title")] public string? Title { get; init; }
    [JsonPropertyName("preview")] public string? Preview { get; init; }
    [JsonPropertyName("summary")] public string? Summary { get; init; }
}

/// <summary>An existing file reference, accepted only inside forwarded messages.</summary>
public sealed class FileOutgoingSegment : OutgoingSegmentBase<FileOutgoingSegmentData>;
public sealed class FileOutgoingSegmentData
{
    [JsonPropertyName("file_id")] public required string FileId { get; init; }
    [JsonPropertyName("file_name")] public required string FileName { get; init; }
    [JsonPropertyName("file_size")] public required long FileSize { get; init; }
    [JsonPropertyName("file_hash")] public string? FileHash { get; init; }
    [JsonPropertyName("url")] public string? Url { get; init; }
}
