using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity.Segment;

[method: JsonConstructor]
public class FaceIncomingSegment(FaceSegmentData data) : IncomingSegmentBase<FaceSegmentData>(data)
{
    public FaceIncomingSegment(string faceId, bool isLarge) : this(new FaceSegmentData(faceId, isLarge)) { }
}

public class FaceOutgoingSegment(FaceSegmentData data) : OutgoingSegmentBase<FaceSegmentData>(data) { }

public class FaceSegmentData(string faceId, bool isLarge = false)
{
    [JsonPropertyName("face_id")]
    public string FaceId { get; } = faceId;

    [JsonPropertyName("is_large")]
    public bool IsLarge { get; } = isLarge;
}
