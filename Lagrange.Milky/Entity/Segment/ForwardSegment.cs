using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity.Segment;

[method: JsonConstructor]
public class ForwardIncomingSegment(ForwardIncomingSegmentData data) : IncomingSegmentBase<ForwardIncomingSegmentData>(data)
{
    public ForwardIncomingSegment(string forwardId, string title, string[] preview, string summary) : this(new ForwardIncomingSegmentData(forwardId, title, preview, summary)) { }
}

public class ForwardIncomingSegmentData(string forwardId, string title, string[] preview, string summary)
{
    [JsonPropertyName("forward_id")]
    public string ForwardId { get; } = forwardId;

    [JsonPropertyName("title")]
    public string Title { get; } = title;

    [JsonPropertyName("preview")]
    public string[] Preview { get; } = preview;

    [JsonPropertyName("summary")]
    public string Summary { get; } = summary;
}

public class ForwardOutgoingSegment(ForwardOutgoingSegmentData data) : OutgoingSegmentBase<ForwardOutgoingSegmentData>(data) { }

public class ForwardOutgoingSegmentData(ForwardOutgoingSegmentDataItem[] messages, string? title = null, string[]? preview = null, string? summary = null, string? prompt = null)
{
    [JsonRequired]
    [JsonPropertyName("messages")]
    public ForwardOutgoingSegmentDataItem[] Messages { get; init; } = messages;

    [JsonPropertyName("title")]
    public string? Title { get; init; } = title;

    [JsonPropertyName("preview")]
    public string[]? Preview { get; init; } = preview;

    [JsonPropertyName("summary")]
    public string? Summary { get; init; } = summary;

    [JsonPropertyName("prompt")]
    public string? Prompt { get; init; } = prompt;
}

public class ForwardOutgoingSegmentDataItem(long userId, string senderName, IOutgoingSegment[] segments)
{
    [JsonPropertyName("user_id")]
    public long UserId { get; init; } = userId;

    [JsonPropertyName("sender_name")]
    public string SenderName { get; init; } = senderName;

    [JsonPropertyName("segments")]
    public IOutgoingSegment[] Segments { get; init; } = segments;
}
