using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Utility;
using Lagrange.Proto;

namespace Lagrange.Core.Message.Entities;

public sealed class BounceFaceEntity : IMessageEntity
{
    public uint FaceId { get; init; }
    public uint Count { get; init; } = 1;
    public string Name { get; init; } = string.Empty;

    public BounceFaceEntity() { }
    public BounceFaceEntity(uint faceId, uint count, string name) => (FaceId, Count, Name) = (faceId, count, name);

    Elem[] IMessageEntity.Build() => [new Elem
    {
        CommonElem = new CommonElem
        {
            ServiceType = 23,
            BusinessType = 13,
            PbElem = ProtoHelper.Serialize(new BounceFaceExtra
            {
                Field1 = 13,
                Count = Count,
                Name = Name,
                Face = new BounceSmallFace { FaceId = FaceId, Text = Name, CompatText = Name }
            })
        }
    }];

    IMessageEntity? IMessageEntity.Parse(List<Elem> elements, Elem target)
    {
        if (target.CommonElem is not { ServiceType: 23, BusinessType: 13 } common) return null;
        try
        {
            var extra = ProtoHelper.Deserialize<BounceFaceExtra>(common.PbElem.Span);
            return new BounceFaceEntity(extra.Face?.FaceId ?? 0, extra.Count, extra.Name ?? extra.Face?.Text ?? string.Empty);
        }
        catch { return null; }
    }

    public string ToPreviewString() => $"[{Name}]x{Count}";
}

[ProtoPackable]
internal partial class BounceFaceExtra
{
    [ProtoMember(1)] public int Field1 { get; set; }
    [ProtoMember(2)] public uint Count { get; set; }
    [ProtoMember(3)] public string Name { get; set; } = string.Empty;
    [ProtoMember(6)] public BounceSmallFace? Face { get; set; }
}

[ProtoPackable]
internal partial class BounceSmallFace
{
    [ProtoMember(1)] public uint FaceId { get; set; }
    [ProtoMember(2)] public string Text { get; set; } = string.Empty;
    [ProtoMember(3)] public string CompatText { get; set; } = string.Empty;
}
