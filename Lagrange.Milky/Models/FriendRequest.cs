using System.Text.Json.Serialization;

namespace Lagrange.Milky.Models;

public sealed class FriendRequest
{
    [JsonPropertyName("flag")] public required string Flag { get; init; }
    [JsonPropertyName("user_id")] public long UserId { get; init; }
    [JsonPropertyName("uid")] public string Uid { get; init; } = string.Empty;
    [JsonPropertyName("nickname")] public string Nickname { get; init; } = string.Empty;
    [JsonPropertyName("message")] public string Message { get; init; } = string.Empty;
    [JsonPropertyName("source")] public string Source { get; init; } = string.Empty;
    [JsonPropertyName("time")] public long Time { get; init; }
    [JsonPropertyName("state")] public string State { get; init; } = "pending";
}
