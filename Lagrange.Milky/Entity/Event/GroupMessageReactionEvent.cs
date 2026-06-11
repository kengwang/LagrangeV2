using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity.Event;

public class GroupMessageReactionEvent(long time, long selfId, GroupMessageReactionEventData data) : EventBase<GroupMessageReactionEventData>(time, selfId, "group_message_reaction", data) { }

public class GroupMessageReactionEventData(long groupId, long userId, long messageSeq, string faceId, string reactionType, bool isAdd)
{
    [JsonPropertyName("group_id")]
    public long GroupId { get; } = groupId;

    [JsonPropertyName("user_id")]
    public long UserId { get; } = userId;

    [JsonPropertyName("message_seq")]
    public long MessageSeq { get; } = messageSeq;

    [JsonPropertyName("face_id")]
    public string FaceId { get; } = faceId;

    [JsonPropertyName("reaction_type")]
    public string ReactionType { get; } = reactionType;

    [JsonPropertyName("is_add")]
    public bool IsAdd { get; } = isAdd;
}
