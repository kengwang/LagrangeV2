using System.Text.Json.Serialization;

namespace Lagrange.Milky.Models.Segments;

public sealed class SpecialPokeIncomingSegment : IncomingSegmentBase<SpecialPokeIncomingSegmentData>;
public sealed class SpecialPokeIncomingSegmentData
{
    [JsonPropertyName("face_id")] public required uint FaceId { get; init; }
    [JsonPropertyName("count")] public required uint Count { get; init; }
    [JsonPropertyName("face_name")] public required string FaceName { get; init; }
}

public sealed class SpecialPokeOutgoingSegment : OutgoingSegmentBase<SpecialPokeOutgoingSegmentData>;
public sealed class SpecialPokeOutgoingSegmentData
{
    [JsonPropertyName("face_id")] public required uint FaceId { get; init; }
    [JsonPropertyName("count")] public uint Count { get; init; }
    [JsonPropertyName("face_name")] public string? FaceName { get; init; }
}
