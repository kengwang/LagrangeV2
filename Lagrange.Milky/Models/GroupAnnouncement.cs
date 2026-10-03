using System.Text.Json.Serialization;

namespace Lagrange.Milky.Models;

public sealed class GroupAnnouncement
{
    [JsonPropertyName("id")] public required string Id { get; init; }
    [JsonPropertyName("publisher_id")] public long PublisherId { get; init; }
    [JsonPropertyName("publish_time")] public long PublishTime { get; init; }
    [JsonPropertyName("text")] public string Text { get; init; } = string.Empty;
    [JsonPropertyName("pinned")] public bool Pinned { get; init; }
    [JsonPropertyName("read_count")] public int ReadCount { get; init; }
}
