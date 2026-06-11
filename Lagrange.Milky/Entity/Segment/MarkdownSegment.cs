using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity.Segment;

[method: JsonConstructor]
public class MarkdownIncomingSegment(MarkdownSegmentData data) : IncomingSegmentBase<MarkdownSegmentData>(data)
{
    public MarkdownIncomingSegment(string content) : this(new MarkdownSegmentData(content)) { }
}

[method: JsonConstructor]
public class MarkdownOutgoingSegment(MarkdownSegmentData data) : OutgoingSegmentBase<MarkdownSegmentData>(data)
{
    public MarkdownOutgoingSegment(string content) : this(new MarkdownSegmentData(content)) { }
}

public class MarkdownSegmentData(string content)
{
    [JsonRequired]
    [JsonPropertyName("content")]
    public string Content { get; init; } = content;
}
