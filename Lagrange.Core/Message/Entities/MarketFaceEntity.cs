using Lagrange.Core.Internal.Packets.Message;

namespace Lagrange.Core.Message.Entities;

public sealed class MarketFaceEntity : IMessageEntity
{
    public int FaceId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;

    Elem[] IMessageEntity.Build() => [new Elem
    {
        MarketFace = new MarketFace
        {
            FaceName = Name,
            FaceInfo = 1,
            ItemType = 6,
            FaceId = TryDecodeFaceId(Url),
            Key = Name,
            TabId = checked((uint)Math.Max(0, FaceId)),
            SubType = 3,
            ImageWidth = 300,
            ImageHeight = 300
        }
    }];

    private static ReadOnlyMemory<byte> TryDecodeFaceId(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length % 2 != 0) return ReadOnlyMemory<byte>.Empty;
        try { return Convert.FromHexString(value); }
        catch (FormatException) { return ReadOnlyMemory<byte>.Empty; }
    }

    IMessageEntity? IMessageEntity.Parse(List<Elem> elements, Elem target)
    {
        if (target.MarketFace is { } face)
            return new MarketFaceEntity
            {
                FaceId = checked((int)face.TabId),
                Name = face.FaceName ?? string.Empty,
                Url = face.FaceId.IsEmpty ? string.Empty : Convert.ToHexString(face.FaceId.Span),
                Summary = string.Empty
            };

        if (target.CustomFace is not { BizType: not 0 } legacy) return null;
        return new MarketFaceEntity
        {
            FaceId = checked((int)legacy.FileId),
            Name = legacy.Shortcut ?? string.Empty,
            Url = legacy.OrigUrl ?? legacy.BigUrl ?? string.Empty,
            Summary = legacy.PbReserve?.Summary ?? string.Empty,
        };
    }

    public string ToPreviewString() => string.IsNullOrWhiteSpace(Name) ? "[商城表情]" : $"[{Name}]";
}
