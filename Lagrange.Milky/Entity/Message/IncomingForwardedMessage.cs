using System.Text.Json.Serialization;
using Lagrange.Milky.Entity.Segment;

namespace Lagrange.Milky.Entity.Message;

public class IncomingForwardedMessage(long messageSeq, string senderName, string avatarUrl, long time, IEnumerable<IIncomingSegment> segments)
{
    [JsonPropertyName("message_seq")]
    public long MessageSeq { get; } = messageSeq;

    [JsonPropertyName("sender_name")]
    public string SenderName { get; } = senderName;

    [JsonPropertyName("avatar_url")]
    public string AvatarUrl { get; } = avatarUrl;

    [JsonPropertyName("time")]
    public long Time { get; } = time;

    [JsonPropertyName("segments")]
    public IEnumerable<IIncomingSegment> Segments { get; } = segments;
}
