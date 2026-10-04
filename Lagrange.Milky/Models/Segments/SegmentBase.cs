using System.Text.Json.Serialization;

namespace Lagrange.Milky.Models.Segments;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextIncomingSegment), "text")]
[JsonDerivedType(typeof(MentionIncomingSegment), "mention")]
[JsonDerivedType(typeof(MentionAllIncomingSegment), "mention_all")]
[JsonDerivedType(typeof(ReplyIncomingSegment), "reply")]
[JsonDerivedType(typeof(ImageIncomingSegment), "image")]
[JsonDerivedType(typeof(RecordIncomingSegment), "record")]
[JsonDerivedType(typeof(VideoIncomingSegment), "video")]
[JsonDerivedType(typeof(FileIncomingSegment), "file")]
[JsonDerivedType(typeof(FlashFileIncomingSegment), "flash_file")]
[JsonDerivedType(typeof(ForwardIncomingSegment), "forward")]
[JsonDerivedType(typeof(LightAppIncomingSegment), "light_app")]
[JsonDerivedType(typeof(FaceIncomingSegment), "face")]
[JsonDerivedType(typeof(XmlIncomingSegment), "xml")]
[JsonDerivedType(typeof(MarketFaceIncomingSegment), "market_face")]
[JsonDerivedType(typeof(PokeIncomingSegment), "poke")]
[JsonDerivedType(typeof(MarkdownIncomingSegment), "markdown")]
[JsonDerivedType(typeof(KeyboardIncomingSegment), "keyboard")]
[JsonDerivedType(typeof(SpecialPokeIncomingSegment), "special_poke")]
[JsonDerivedType(typeof(JsonIncomingSegment), "json")]
[JsonDerivedType(typeof(LocationIncomingSegment), "location")]
[JsonDerivedType(typeof(MusicIncomingSegment), "music")]
[JsonDerivedType(typeof(ShareIncomingSegment), "share")]
[JsonDerivedType(typeof(ContactIncomingSegment), "contact")]
[JsonDerivedType(typeof(DiceIncomingSegment), "dice")]
[JsonDerivedType(typeof(RpsIncomingSegment), "rps")]
[JsonDerivedType(typeof(LongMsgIncomingSegment), "long_msg")]
[JsonDerivedType(typeof(StreamIncomingSegment), "stream")]
[JsonDerivedType(typeof(BounceFaceIncomingSegment), "bounce_face")]
[JsonDerivedType(typeof(GroupReactionIncomingSegment), "group_reaction")]
[JsonDerivedType(typeof(GreyTipIncomingSegment), "grey_tip")]
public abstract class IncomingSegmentBase;
public abstract class IncomingSegmentBase<T> : IncomingSegmentBase
{
    [JsonPropertyName("data")] public required T Data { get; init; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextOutgoingSegment), "text")]
[JsonDerivedType(typeof(MentionOutgoingSegment), "mention")]
[JsonDerivedType(typeof(MentionAllOutgoingSegment), "mention_all")]
[JsonDerivedType(typeof(ReplyOutgoingSegment), "reply")]
[JsonDerivedType(typeof(ImageOutgoingSegment), "image")]
[JsonDerivedType(typeof(RecordOutgoingSegment), "record")]
[JsonDerivedType(typeof(VideoOutgoingSegment), "video")]
[JsonDerivedType(typeof(FileOutgoingSegment), "file")]
[JsonDerivedType(typeof(ForwardOutgoingSegment), "forward")]
[JsonDerivedType(typeof(LightAppOutgoingSegment), "light_app")]
[JsonDerivedType(typeof(FaceOutgoingSegment), "face")]
[JsonDerivedType(typeof(MarketFaceOutgoingSegment), "market_face")]
[JsonDerivedType(typeof(XmlOutgoingSegment), "xml")]
[JsonDerivedType(typeof(PokeOutgoingSegment), "poke")]
[JsonDerivedType(typeof(MarkdownOutgoingSegment), "markdown")]
[JsonDerivedType(typeof(KeyboardOutgoingSegment), "keyboard")]
[JsonDerivedType(typeof(SpecialPokeOutgoingSegment), "special_poke")]
[JsonDerivedType(typeof(JsonOutgoingSegment), "json")]
[JsonDerivedType(typeof(LocationOutgoingSegment), "location")]
[JsonDerivedType(typeof(MusicOutgoingSegment), "music")]
[JsonDerivedType(typeof(ShareOutgoingSegment), "share")]
[JsonDerivedType(typeof(ContactOutgoingSegment), "contact")]
[JsonDerivedType(typeof(DiceOutgoingSegment), "dice")]
[JsonDerivedType(typeof(RpsOutgoingSegment), "rps")]
[JsonDerivedType(typeof(LongMsgOutgoingSegment), "long_msg")]
[JsonDerivedType(typeof(StreamOutgoingSegment), "stream")]
[JsonDerivedType(typeof(BounceFaceOutgoingSegment), "bounce_face")]
[JsonDerivedType(typeof(GroupReactionOutgoingSegment), "group_reaction")]
[JsonDerivedType(typeof(GreyTipOutgoingSegment), "grey_tip")]
public abstract class OutgoingSegmentBase;
public abstract class OutgoingSegmentBase<T> : OutgoingSegmentBase
{
    [JsonPropertyName("data")] public required T Data { get; init; }
}
