using Lagrange.Core.Internal.Packets.Message;

namespace Lagrange.Core.Message.Entities;

public class MarketFaceEntity(string emojiId, int emojiPackageId, string key, string summary, string url) : IMessageEntity
{
    public string EmojiId { get; set; } = emojiId;

    public int EmojiPackageId { get; set; } = emojiPackageId;

    public string Key { get; set; } = key;

    public string Summary { get; set; } = summary;

    public string Url { get; set; } = url;

    public MarketFaceEntity() : this(string.Empty, 0, string.Empty, string.Empty, string.Empty) { }

    Elem[] IMessageEntity.Build()
    {
        return
        [
            new Elem
            {
                Marketface = new Marketface
                {
                    Summary = Summary,
                    ItemType = 6,
                    Info = 1,
                    FaceId = Convert.FromHexString(EmojiId),
                    TabId = EmojiPackageId,
                    SubType = 3,
                    Key = Key,
                    Width = 300,
                    Height = 300,
                    PbReserve = new MarketfaceReserve { Field8 = 1 }
                }
            }
        ];
    }

    IMessageEntity? IMessageEntity.Parse(List<Elem> elements, Elem target)
    {
        if (target.Marketface is not { } marketFace) return null;

        return new MarketFaceEntity(
            Convert.ToHexString(marketFace.FaceId).ToLowerInvariant(),
            marketFace.TabId,
            marketFace.Key,
            marketFace.Summary,
            string.Empty
        );
    }

    string IMessageEntity.ToPreviewString() => Summary;
}
