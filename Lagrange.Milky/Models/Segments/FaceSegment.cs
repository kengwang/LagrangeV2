using System.Text.Json.Serialization;

namespace Lagrange.Milky.Models.Segments;

public sealed class FaceIncomingSegment : IncomingSegmentBase<FaceIncomingSegmentData>;

public sealed class FaceIncomingSegmentData
{
    [JsonPropertyName("large")] public bool Large { get; init; } = true;
    [JsonPropertyName("result_id")] public string? ResultId { get; init; }
    [JsonPropertyName("face_id")] public required uint FaceId { get; init; }
    [JsonPropertyName("raw")] public required string Raw { get; init; }
}

public sealed class FaceOutgoingSegment : OutgoingSegmentBase<FaceOutgoingSegmentData>;

public sealed class FaceOutgoingSegmentData
{
    [JsonPropertyName("large")] public bool Large { get; init; } = true;
    [JsonPropertyName("result_id")] public string? ResultId { get; init; }
    [JsonPropertyName("face_id")] public required uint FaceId { get; init; }
    [JsonPropertyName("raw")] public string? Raw { get; init; }
}
