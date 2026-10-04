using System.Text.Json.Serialization;

namespace Lagrange.Milky.Models.Segments;

public sealed class KeyboardIncomingSegment : IncomingSegmentBase<KeyboardIncomingSegmentData>;
public sealed class KeyboardIncomingSegmentData
{
    [JsonPropertyName("data")] public required string Data { get; init; }
}

public sealed class KeyboardOutgoingSegment : OutgoingSegmentBase<KeyboardOutgoingSegmentData>;
public sealed class KeyboardOutgoingSegmentData
{
    [JsonPropertyName("data")] public required string Data { get; init; }
}
