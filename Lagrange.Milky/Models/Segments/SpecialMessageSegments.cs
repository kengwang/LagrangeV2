using System.Text.Json.Serialization;
using System.Collections.Generic;
using Lagrange.Milky.Models.Messages;

namespace Lagrange.Milky.Models.Segments;

public sealed class DiceIncomingSegment : IncomingSegmentBase<DiceIncomingSegmentData>;
public sealed class DiceIncomingSegmentData { [JsonPropertyName("face_id")] public required int FaceId { get; init; } }
public sealed class DiceOutgoingSegment : OutgoingSegmentBase<DiceOutgoingSegmentData>;
public sealed class DiceOutgoingSegmentData { [JsonPropertyName("face_id")] public int FaceId { get; init; } = 358; }

public sealed class RpsIncomingSegment : IncomingSegmentBase<RpsIncomingSegmentData>;
public sealed class RpsIncomingSegmentData { [JsonPropertyName("face_id")] public required int FaceId { get; init; } }
public sealed class RpsOutgoingSegment : OutgoingSegmentBase<RpsOutgoingSegmentData>;
public sealed class RpsOutgoingSegmentData { [JsonPropertyName("face_id")] public int FaceId { get; init; } = 359; }

public sealed class LongMsgIncomingSegment : IncomingSegmentBase<LongMsgIncomingSegmentData>;
public sealed class LongMsgIncomingSegmentData { [JsonPropertyName("res_id")] public required string ResId { get; init; } [JsonPropertyName("messages")] public IReadOnlyList<IncomingForwardedMessage>? Messages { get; init; } }
public sealed class LongMsgOutgoingSegment : OutgoingSegmentBase<LongMsgOutgoingSegmentData>;
public sealed class LongMsgOutgoingSegmentData { [JsonPropertyName("res_id")] public required string ResId { get; init; } }

public sealed class StreamIncomingSegment : IncomingSegmentBase<StreamIncomingSegmentData>;
public sealed class StreamIncomingSegmentData { [JsonPropertyName("text")] public required string Text { get; init; } }
public sealed class StreamOutgoingSegment : OutgoingSegmentBase<StreamOutgoingSegmentData>;
public sealed class StreamOutgoingSegmentData { [JsonPropertyName("text")] public required string Text { get; init; } }
