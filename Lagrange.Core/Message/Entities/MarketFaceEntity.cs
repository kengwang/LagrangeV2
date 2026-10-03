using Lagrange.Core.Internal.Packets.Message;

namespace Lagrange.Core.Message.Entities;

public sealed class MarketFaceEntity : IMessageEntity
{
    public int FaceId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;

    Elem[] IMessageEntity.Build() => [new Elem { CustomFace = new CustomFace { FileId = checked((uint)Math.Max(0, FaceId)), Shortcut = Name, OrigUrl = Url, BizType = 1 } }];

    IMessageEntity? IMessageEntity.Parse(List<Elem> elements, Elem target)
    {
        if (target.CustomFace is not { BizType: not 0 } face) return null;
        return new MarketFaceEntity
        {
            FaceId = checked((int)face.FileId),
            Name = face.Shortcut ?? string.Empty,
            Url = face.OrigUrl ?? face.BigUrl ?? string.Empty,
            Summary = face.PbReserve?.Summary ?? string.Empty,
        };
    }

    public string ToPreviewString() => string.IsNullOrWhiteSpace(Name) ? "[商城表情]" : $"[{Name}]";
}
