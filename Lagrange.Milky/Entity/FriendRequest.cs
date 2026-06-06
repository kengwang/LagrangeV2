using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity;

public class FriendRequest(long time, long initiatorId, string initiatorUid, long targetUserId, string targetUserUid, string state, string comment, string via, bool isFiltered)
{
    [JsonPropertyName("time")]
    public long Time { get; } = time;

    [JsonPropertyName("initiator_id")]
    public long InitiatorId { get; } = initiatorId;

    [JsonPropertyName("initiator_uid")]
    public string InitiatorUid { get; } = initiatorUid;

    [JsonPropertyName("target_user_id")]
    public long TargetUserId { get; } = targetUserId;

    [JsonPropertyName("target_user_uid")]
    public string TargetUserUid { get; } = targetUserUid;

    [JsonPropertyName("state")]
    public string State { get; } = state;

    [JsonPropertyName("comment")]
    public string Comment { get; } = comment;

    [JsonPropertyName("via")]
    public string Via { get; } = via;

    [JsonPropertyName("is_filtered")]
    public bool IsFiltered { get; } = isFiltered;
}
