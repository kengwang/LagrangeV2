using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity.Segment;

[method: JsonConstructor]
public class GeneralFlagsIncomingSegment(GeneralFlagsSegmentData data) : IncomingSegmentBase<GeneralFlagsSegmentData>(data)
{
    public GeneralFlagsIncomingSegment(uint bubbleId, uint bubbleSubId, ulong pendantId) : this(new GeneralFlagsSegmentData(bubbleId, bubbleSubId, pendantId)) { }
}

public class GeneralFlagsOutgoingSegment(GeneralFlagsSegmentData data) : OutgoingSegmentBase<GeneralFlagsSegmentData>(data) { }

public class GeneralFlagsSegmentData(uint bubbleId, uint bubbleSubId, ulong pendantId)
{
    [JsonPropertyName("bubble_id")]
    public uint BubbleId { get; set; } = bubbleId;

    [JsonPropertyName("bubble_sub_id")]
    public uint BubbleSubId { get; set; } = bubbleSubId;

    [JsonPropertyName("pendant_id")]
    public ulong PendantId { get; set; } = pendantId;
}