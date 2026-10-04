using System.Text.Json.Serialization;

namespace Lagrange.Milky.Models.Segments;

public sealed class GreyTipIncomingSegment : IncomingSegmentBase<GreyTipSegmentData>;

public sealed class GreyTipOutgoingSegment : OutgoingSegmentBase<GreyTipSegmentData>;

public sealed class GreyTipSegmentData
{
    [JsonPropertyName("text")] public required string Text { get; init; }
}
