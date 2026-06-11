using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity.Segment;

[method: JsonConstructor]
public class MarketFaceIncomingSegment(MarketFaceSegmentData data) : IncomingSegmentBase<MarketFaceSegmentData>(data)
{
    public MarketFaceIncomingSegment(int emojiPackageId, string emojiId, string key, string summary, string url) : this(new MarketFaceSegmentData(emojiPackageId, emojiId, key, summary, url)) { }
}

public class MarketFaceSegmentData(int emojiPackageId, string emojiId, string key, string summary, string url)
{
    [JsonPropertyName("emoji_package_id")]
    public int EmojiPackageId { get; } = emojiPackageId;

    [JsonPropertyName("emoji_id")]
    public string EmojiId { get; } = emojiId;

    [JsonPropertyName("key")]
    public string Key { get; } = key;

    [JsonPropertyName("summary")]
    public string Summary { get; } = summary;

    [JsonPropertyName("url")]
    public string Url { get; } = url;
}
