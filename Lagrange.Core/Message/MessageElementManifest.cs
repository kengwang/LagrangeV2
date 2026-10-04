namespace Lagrange.Core.Message;

/// <summary>Supported direction of a QQ message entity and its Milky segment.</summary>
[Flags]
public enum MessageElementDirections
{
    /// <summary>No supported direction.</summary>
    None = 0,
    /// <summary>QQ protocol decoding.</summary>
    Decode = 1,
    /// <summary>Milky incoming event conversion.</summary>
    Report = 2,
    /// <summary>Milky outgoing input conversion.</summary>
    Input = 4,
    /// <summary>QQ protocol encoding.</summary>
    Encode = 8,
    /// <summary>All four directions.</summary>
    All = Decode | Report | Input | Encode
}

/// <summary>A directional capability declaration; restrictions describe the supported send context.</summary>
public sealed record MessageElementCapability(string Segment, string Entity, MessageElementDirections Directions, string? Restriction = null);

/// <summary>Direction inventory shared by protocol and Milky contract tests.</summary>
public static class MessageElementManifest
{
    /// <summary>The supported entities and segment directions, including deliberate receive-only entries.</summary>
    public static IReadOnlyList<MessageElementCapability> Entries { get; } = Array.AsReadOnly<MessageElementCapability>(
    [
        new("text", "TextEntity", MessageElementDirections.All),
        new("mention", "MentionEntity", MessageElementDirections.All),
        new("mention_all", "MentionEntity", MessageElementDirections.All),
        new("reply", "ReplyEntity", MessageElementDirections.All),
        new("image", "ImageEntity", MessageElementDirections.All, "Flash semantics are receive only; normal images support all directions."),
        new("record", "RecordEntity", MessageElementDirections.All),
        new("video", "VideoEntity", MessageElementDirections.All),
        new("file", "GroupFileEntity", MessageElementDirections.All, "Outgoing references only inside forwards; live files use dedicated upload APIs."),
        new("flash_file", "FlashFileEntity", MessageElementDirections.Decode | MessageElementDirections.Report, "Receive only; send through the flash transfer API."),
        new("forward", "MultiMsgEntity", MessageElementDirections.All),
        new("light_app", "LightAppEntity", MessageElementDirections.All),
        new("face", "FaceEntity", MessageElementDirections.All, "Animated sends require catalog metadata."),
        new("dice", "FaceEntity", MessageElementDirections.All, "Face 358 alias retained."),
        new("rps", "FaceEntity", MessageElementDirections.All, "Face 359 alias retained."),
        new("xml", "XmlEntity", MessageElementDirections.All),
        new("market_face", "MarketFaceEntity", MessageElementDirections.All),
        new("poke", "PokeEntity", MessageElementDirections.All, "Send only as the sole segment in a direct private session."),
        new("markdown", "MarkdownEntity", MessageElementDirections.All),
        new("keyboard", "KeyboardEntity", MessageElementDirections.All),
        new("special_poke", "SpecialPokeEntity", MessageElementDirections.All),
        new("json", "JsonEntity", MessageElementDirections.All),
        new("location", "LocationEntity", MessageElementDirections.All),
        new("music", "MusicEntity", MessageElementDirections.All),
        new("share", "ShareEntity", MessageElementDirections.All),
        new("contact", "ContactEntity", MessageElementDirections.All),
        new("long_msg", "LongMsgEntity", MessageElementDirections.All),
        new("stream", "StreamEntity", MessageElementDirections.All),
        new("bounce_face", "BounceFaceEntity", MessageElementDirections.All),
        new("group_reaction", "GroupReactionEntity", MessageElementDirections.All),
        new("grey_tip", "GreyTipEntity", MessageElementDirections.All),
    ]);
}
