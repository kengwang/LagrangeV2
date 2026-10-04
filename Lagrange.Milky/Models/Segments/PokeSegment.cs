using System.Text.Json.Serialization;

namespace Lagrange.Milky.Models.Segments;

public sealed class PokeIncomingSegment : IncomingSegmentBase<PokeIncomingSegmentData>;
public sealed class PokeIncomingSegmentData
{
    [JsonPropertyName("type")] public required uint Type { get; init; }
    [JsonPropertyName("strength")] public uint Strength { get; init; }
}

public sealed class PokeOutgoingSegment : OutgoingSegmentBase<PokeOutgoingSegmentData>;
public sealed class PokeOutgoingSegmentData
{
    [JsonPropertyName("type")] public required uint Type { get; init; }
    [JsonPropertyName("strength")] public uint Strength { get; init; }
}
