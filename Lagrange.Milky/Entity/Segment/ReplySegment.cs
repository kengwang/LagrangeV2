using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity.Segment;

[method: JsonConstructor]
public class ReplyIncomingSegment(ReplyIncomingSegmentData data) : IncomingSegmentBase<ReplyIncomingSegmentData>(data)
{
    public ReplyIncomingSegment(long messageSeq, long senderId, string? senderName, long time, IReadOnlyList<IIncomingSegment> segments)
        : this(new ReplyIncomingSegmentData(messageSeq, senderId, senderName, time, segments)) { }
}

public class ReplyOutgoingSegment(ReplySegmentData data) : OutgoingSegmentBase<ReplySegmentData>(data) { }

public class ReplyIncomingSegmentData(long messageSeq, long senderId, string? senderName, long time, IReadOnlyList<IIncomingSegment> segments)
{
    [JsonPropertyName("message_seq")]
    public long MessageSeq { get; } = messageSeq;

    [JsonPropertyName("sender_id")]
    public long SenderId { get; } = senderId;

    [JsonPropertyName("sender_name")]
    public string? SenderName { get; } = senderName;

    [JsonPropertyName("time")]
    public long Time { get; } = time;

    [JsonPropertyName("segments")]
    public IReadOnlyList<IIncomingSegment> Segments { get; } = segments;
}

public class ReplySegmentData(long messageSeq)
{
    [JsonPropertyName("message_seq")]
    public long MessageSeq { get; } = messageSeq;
}
