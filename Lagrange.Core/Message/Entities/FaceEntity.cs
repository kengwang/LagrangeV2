using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Utility;
using Lagrange.Proto;

namespace Lagrange.Core.Message.Entities;

/// <summary>A QQ system-face element.</summary>
public sealed class FaceEntity : IMessageEntity
{
    public int FaceId { get; init; }
    public string Raw { get; init; } = string.Empty;

    Elem[] IMessageEntity.Build() => FaceId >= 260
        ? [new Elem
        {
            CommonElem = new CommonElem
            {
                ServiceType = 33,
                BusinessType = 1,
                PbElem = ProtoHelper.Serialize(new SmallFaceExtra { FaceId = checked((uint)FaceId) })
            }
        }]
        : [new Elem { Face = new Face { Index = FaceId } }];

    IMessageEntity? IMessageEntity.Parse(List<Elem> elements, Elem target)
    {
        if (target.Face is { } face)
            return new FaceEntity { FaceId = face.Index, Raw = string.IsNullOrEmpty(Raw) ? $"face{face.Index}" : Raw };

        if (target.CommonElem is { ServiceType: 33 } small)
        {
            try { return new FaceEntity { FaceId = checked((int)ProtoHelper.Deserialize<SmallFaceExtra>(small.PbElem.Span).FaceId) }; }
            catch { return null; }
        }

        // Keep compatibility with legacy clients which still send CustomFace.
        if (target.CustomFace is { BizType: 0 } legacy)
            return new FaceEntity { FaceId = checked((int)legacy.FileId), Raw = legacy.Shortcut ?? string.Empty };

        return null;
    }

    public string ToPreviewString() => $"[{Raw}]";
}

[ProtoPackable]
internal partial class SmallFaceExtra
{
    [ProtoMember(1)] public uint FaceId { get; set; }
    [ProtoMember(2)] public string Preview { get; set; } = string.Empty;
    [ProtoMember(3)] public string Preview2 { get; set; } = string.Empty;
}
