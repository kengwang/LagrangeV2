using System.Text.Json.Serialization;

namespace Lagrange.Milky.Models.Segments;

public sealed class JsonIncomingSegment : IncomingSegmentBase<JsonIncomingSegmentData>;
public sealed class JsonIncomingSegmentData { [JsonPropertyName("data")] public required string Data { get; init; } }
public sealed class JsonOutgoingSegment : OutgoingSegmentBase<JsonOutgoingSegmentData>;
public sealed class JsonOutgoingSegmentData { [JsonPropertyName("data")] public required string Data { get; init; } }

public sealed class LocationIncomingSegment : IncomingSegmentBase<LocationIncomingSegmentData>;
public sealed class LocationIncomingSegmentData { [JsonPropertyName("latitude")] public required string Latitude { get; init; } [JsonPropertyName("longitude")] public required string Longitude { get; init; } [JsonPropertyName("title")] public required string Title { get; init; } [JsonPropertyName("content")] public required string Content { get; init; } }
public sealed class LocationOutgoingSegment : OutgoingSegmentBase<LocationOutgoingSegmentData>;
public sealed class LocationOutgoingSegmentData { [JsonPropertyName("latitude")] public required string Latitude { get; init; } [JsonPropertyName("longitude")] public required string Longitude { get; init; } [JsonPropertyName("title")] public string? Title { get; init; } [JsonPropertyName("content")] public string? Content { get; init; } }

public sealed class MusicIncomingSegment : IncomingSegmentBase<MusicIncomingSegmentData>;
public class MusicIncomingSegmentData { [JsonPropertyName("type")] public required string Type { get; init; } [JsonPropertyName("id")] public required string Id { get; init; } [JsonPropertyName("url")] public required string Url { get; init; } [JsonPropertyName("audio")] public required string Audio { get; init; } [JsonPropertyName("title")] public required string Title { get; init; } [JsonPropertyName("content")] public required string Content { get; init; } [JsonPropertyName("image")] public required string Image { get; init; } }
public sealed class MusicOutgoingSegment : OutgoingSegmentBase<MusicOutgoingSegmentData>;
public sealed class MusicOutgoingSegmentData : MusicIncomingSegmentData;

public sealed class ShareIncomingSegment : IncomingSegmentBase<ShareIncomingSegmentData>;
public class ShareIncomingSegmentData { [JsonPropertyName("url")] public required string Url { get; init; } [JsonPropertyName("title")] public required string Title { get; init; } [JsonPropertyName("content")] public required string Content { get; init; } [JsonPropertyName("image")] public required string Image { get; init; } }
public sealed class ShareOutgoingSegment : OutgoingSegmentBase<ShareOutgoingSegmentData>;
public sealed class ShareOutgoingSegmentData : ShareIncomingSegmentData;

public sealed class ContactIncomingSegment : IncomingSegmentBase<ContactIncomingSegmentData>;
public class ContactIncomingSegmentData { [JsonPropertyName("contact_type")] public required string ContactType { get; init; } [JsonPropertyName("id")] public required string Id { get; init; } }
public sealed class ContactOutgoingSegment : OutgoingSegmentBase<ContactOutgoingSegmentData>;
public sealed class ContactOutgoingSegmentData : ContactIncomingSegmentData;


