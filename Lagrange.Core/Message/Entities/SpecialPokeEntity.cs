using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Utility;
using Lagrange.Proto;

namespace Lagrange.Core.Message.Entities;

/// <summary>Special poke/bounce-face element (CommonElem service 23).</summary>
public sealed class SpecialPokeEntity : IMessageEntity
{
    public uint FaceId { get; init; }
    public uint Count { get; init; }
    public string FaceName { get; init; } = string.Empty;

    public SpecialPokeEntity() { }

    public SpecialPokeEntity(uint faceId, uint count, string faceName)
    {
        FaceId = faceId;
        Count = count;
        FaceName = faceName;
    }

    Elem[] IMessageEntity.Build() => [new Elem
    {
        CommonElem = new CommonElem
        {
            ServiceType = 23,
            BusinessType = FaceId,
            PbElem = ProtoHelper.Serialize(new SpecialPokeExtra
            {
                Type = FaceId,
                Count = Count,
                FaceName = FaceName
            })
        }
    }];

    IMessageEntity? IMessageEntity.Parse(List<Elem> elements, Elem target)
    {
        if (target.CommonElem is not { ServiceType: 23 } common) return null;
        try
        {
            var extra = ProtoHelper.Deserialize<SpecialPokeExtra>(common.PbElem.Span);
            return new SpecialPokeEntity(extra.Type, extra.Count, extra.FaceName ?? string.Empty);
        }
        catch { return null; }
    }

    public string ToPreviewString() => $"[{FaceName}]x{Count}";
}

[ProtoPackable]
internal partial class SpecialPokeExtra
{
    [ProtoMember(1)] public uint Type { get; set; }
    [ProtoMember(2)] public uint Count { get; set; }
    [ProtoMember(3)] public string FaceName { get; set; } = string.Empty;
}
