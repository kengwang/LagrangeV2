using System.Text.Json.Serialization;

namespace Lagrange.Milky.Models.Segments;

public sealed class FaceIncomingSegment : IncomingSegmentBase<FaceIncomingSegmentData>;

public sealed class FaceIncomingSegmentData
{
    [JsonPropertyName("face_id")] public required int FaceId { get; init; }
    [JsonPropertyName("raw")] public required string Raw { get; init; }
}

public sealed class FaceOutgoingSegment : OutgoingSegmentBase<FaceOutgoingSegmentData>;

public sealed class FaceOutgoingSegmentData
{
    [JsonPropertyName("face_id")] public required int FaceId { get; init; }
    [JsonPropertyName("raw")] public string? Raw { get; init; }
}
