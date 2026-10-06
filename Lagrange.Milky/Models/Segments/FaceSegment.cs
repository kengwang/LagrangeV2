using System.Text.Json.Serialization;

namespace Lagrange.Milky.Models.Segments;

public sealed class FaceIncomingSegment : IncomingSegmentBase<FaceIncomingSegmentData>;

public sealed class FaceIncomingSegmentData
{
    [JsonPropertyName("is_large")] public bool Large { get; init; }
    [JsonPropertyName("result_id")] public string? ResultId { get; init; }
    [JsonPropertyName("face_id")] public required string FaceId { get; init; }
    [JsonPropertyName("raw")] public string Raw { get; init; } = string.Empty;
}

public sealed class FaceOutgoingSegment : OutgoingSegmentBase<FaceOutgoingSegmentData>;

public sealed class FaceOutgoingSegmentData
{
    [JsonPropertyName("is_large")] public bool Large { get; init; }
    [JsonPropertyName("result_id")] public string? ResultId { get; init; }
    [JsonPropertyName("face_id")] public required string FaceId { get; init; }
    [JsonPropertyName("raw")] public string? Raw { get; init; }
}
