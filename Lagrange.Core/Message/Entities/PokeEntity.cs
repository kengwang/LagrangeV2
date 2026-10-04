using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Utility;
using Lagrange.Proto;

namespace Lagrange.Core.Message.Entities;

/// <summary>QQ NT window-shake (poke) message element.</summary>
public sealed class PokeEntity : IMessageEntity
{
    public uint Type { get; init; }

    public uint Strength { get; init; }

    public PokeEntity() { }

    public PokeEntity(uint type, uint strength = 0)
    {
        Type = type;
        Strength = strength;
    }

    Elem[] IMessageEntity.Build() => [new Elem
    {
        CommonElem = new CommonElem
        {
            ServiceType = 2,
            BusinessType = Type,
            PbElem = ProtoHelper.Serialize(new PokeExtra { Type = Type, Strength = Strength })
        }
    }];

    IMessageEntity? IMessageEntity.Parse(List<Elem> elements, Elem target)
    {
        if (target.CommonElem is not { ServiceType: 2 } common) return null;
        var type = common.BusinessType;
        if (!common.PbElem.IsEmpty)
        {
            try
            {
                var poke = ProtoHelper.Deserialize<PokeExtra>(common.PbElem.Span);
                return new PokeEntity(poke.Type, poke.Strength);
            }
            catch { /* retain the discriminator when an old client omits pbElem */ }
        }
        return new PokeEntity(type);
    }

    public string ToPreviewString() => "[戳一戳]";
}

[ProtoPackable]
internal partial class PokeExtra
{
    [ProtoMember(1)] public uint Type { get; set; }

    [ProtoMember(7)] public uint Strength { get; set; }
}
