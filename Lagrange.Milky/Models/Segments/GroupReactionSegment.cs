using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Lagrange.Milky.Models.Segments;

public sealed class GroupReactionIncomingSegment : IncomingSegmentBase<GroupReactionIncomingSegmentData>;
public sealed class GroupReactionIncomingSegmentData
{
    [JsonPropertyName("reactions")] public required IReadOnlyList<GroupReactionItem> Reactions { get; init; }
}
public sealed class GroupReactionOutgoingSegment : OutgoingSegmentBase<GroupReactionOutgoingSegmentData>;
public sealed class GroupReactionOutgoingSegmentData
{
    [JsonPropertyName("reactions")] public required IReadOnlyList<GroupReactionItem> Reactions { get; init; }
}
public sealed class GroupReactionItem
{
    [JsonPropertyName("face_id")] public required string FaceId { get; init; }
    [JsonPropertyName("type")] public uint Type { get; init; }
    [JsonPropertyName("count")] public uint Count { get; init; }
    [JsonPropertyName("is_added")] public bool IsAdded { get; init; }
}
