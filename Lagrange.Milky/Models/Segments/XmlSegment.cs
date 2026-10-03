using System.Text.Json.Serialization;

namespace Lagrange.Milky.Models.Segments;

public sealed class XmlIncomingSegment : IncomingSegmentBase<XmlIncomingSegmentData>;

public sealed class XmlIncomingSegmentData
{
    [JsonPropertyName("xml")] public required string Xml { get; init; }
}

public sealed class XmlOutgoingSegment : OutgoingSegmentBase<XmlOutgoingSegmentData>;

public sealed class XmlOutgoingSegmentData
{
    [JsonPropertyName("xml")] public required string Xml { get; init; }
}
