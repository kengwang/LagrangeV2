using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity.Segment;

[method: JsonConstructor]
public class ElemFlags2IncomingSegment(ElemFlags2SegmentData data) : IncomingSegmentBase<ElemFlags2SegmentData>(data)
{
    public ElemFlags2IncomingSegment(uint bubbleId) : this(new ElemFlags2SegmentData(bubbleId)) { }
}

public class ElemFlags2OutgoingSegment(ElemFlags2SegmentData data) : OutgoingSegmentBase<ElemFlags2SegmentData>(data) { }

public class ElemFlags2SegmentData(uint bubbleId)
{
    [JsonPropertyName("bubbleId")]
    public uint BubbleId { get; } = bubbleId;
}