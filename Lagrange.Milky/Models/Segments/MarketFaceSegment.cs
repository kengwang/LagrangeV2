using System.Text.Json.Serialization;

namespace Lagrange.Milky.Models.Segments;

public sealed class MarketFaceIncomingSegment : IncomingSegmentBase<MarketFaceIncomingSegmentData>;

public sealed class MarketFaceIncomingSegmentData
{
    [JsonPropertyName("face_id")] public required uint FaceId { get; init; }
    [JsonPropertyName("name")] public required string Name { get; init; }
    [JsonPropertyName("url")] public required string Url { get; init; }
    [JsonPropertyName("summary")] public required string Summary { get; init; }
}

public sealed class MarketFaceOutgoingSegment : OutgoingSegmentBase<MarketFaceOutgoingSegmentData>;

public sealed class MarketFaceOutgoingSegmentData
{
    [JsonPropertyName("face_id")] public required uint FaceId { get; init; }
    [JsonPropertyName("name")] public string? Name { get; init; }
    [JsonPropertyName("url")] public string? Url { get; init; }
    [JsonPropertyName("summary")] public string? Summary { get; init; }
}
