using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity.Event;

public class GroupJoinRequestEvent(long time, long selfId, GroupJoinRequestEventData data) : EventBase<GroupJoinRequestEventData>(time, selfId, "group_join_request", data) { }

public class GroupJoinRequestEventData(long groupId, long notificationSeq, bool isFiltered, long initiatorId, string comment)
{
    [JsonPropertyName("group_id")]
    public long GroupId { get; } = groupId;

    [JsonPropertyName("notification_seq")]
    public long NotificationSeq { get; } = notificationSeq;

    [JsonPropertyName("is_filtered")]
    public bool IsFiltered { get; } = isFiltered;

    [JsonPropertyName("initiator_id")]
    public long InitiatorId { get; } = initiatorId;

    [JsonPropertyName("comment")]
    public string Comment { get; } = comment;
}
