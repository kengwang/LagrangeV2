using Lagrange.Core.Internal.Packets.Message;

namespace Lagrange.Core.Message.Entities;

/// <summary>A QQ system-face element.</summary>
public sealed class FaceEntity : IMessageEntity
{
    public int FaceId { get; init; }
    public string Raw { get; init; } = string.Empty;

    Elem[] IMessageEntity.Build() => [new Elem { CustomFace = new CustomFace { FileId = checked((uint)Math.Max(0, FaceId)), Shortcut = Raw } }];

    IMessageEntity? IMessageEntity.Parse(List<Elem> elements, Elem target)
    {
        if (target.CustomFace is not { BizType: 0 } face) return null;
        return new FaceEntity { FaceId = checked((int)face.FileId), Raw = face.Shortcut ?? string.Empty };
    }

    public string ToPreviewString() => $"[{Raw}]";
}
