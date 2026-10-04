using System.Text.Json.Serialization;

namespace Lagrange.Milky.Models.Segments;

public sealed class MarkdownIncomingSegment : IncomingSegmentBase<MarkdownIncomingSegmentData>;
public sealed class MarkdownIncomingSegmentData
{
    [JsonPropertyName("content")] public required string Content { get; init; }
}

public sealed class MarkdownOutgoingSegment : OutgoingSegmentBase<MarkdownOutgoingSegmentData>;
public sealed class MarkdownOutgoingSegmentData
{
    [JsonPropertyName("content")] public required string Content { get; init; }
}
