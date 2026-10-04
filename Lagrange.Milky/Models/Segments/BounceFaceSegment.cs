using System.Text.Json.Serialization;

namespace Lagrange.Milky.Models.Segments;

public sealed class BounceFaceIncomingSegment : IncomingSegmentBase<BounceFaceIncomingSegmentData>;
public sealed class BounceFaceIncomingSegmentData
{
    [JsonPropertyName("face_id")] public required uint FaceId { get; init; }
    [JsonPropertyName("count")] public required uint Count { get; init; }
    [JsonPropertyName("name")] public required string Name { get; init; }
}

public sealed class BounceFaceOutgoingSegment : OutgoingSegmentBase<BounceFaceOutgoingSegmentData>;
public sealed class BounceFaceOutgoingSegmentData
{
    [JsonPropertyName("face_id")] public required uint FaceId { get; init; }
    [JsonPropertyName("count")] public uint Count { get; init; } = 1;
    [JsonPropertyName("name")] public string? Name { get; init; }
}
