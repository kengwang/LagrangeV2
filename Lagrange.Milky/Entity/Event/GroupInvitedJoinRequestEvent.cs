using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity.Event;

public class GroupInvitedJoinRequestEvent(long time, long selfId, GroupInvitedJoinRequestEventData data) : EventBase<GroupInvitedJoinRequestEventData>(time, selfId, "group_invited_join_request", data) { }

public class GroupInvitedJoinRequestEventData(long groupId, long notificationSeq, long initiatorId, long targetUserId)
{
    [JsonPropertyName("group_id")]
    public long GroupId { get; } = groupId;

    [JsonPropertyName("notification_seq")]
    public long NotificationSeq { get; } = notificationSeq;

    [JsonPropertyName("initiator_id")]
    public long InitiatorId { get; } = initiatorId;

    [JsonPropertyName("target_user_id")]
    public long TargetUserId { get; } = targetUserId;
}
