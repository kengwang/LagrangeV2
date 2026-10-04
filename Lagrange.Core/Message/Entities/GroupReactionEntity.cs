using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Utility;
using Lagrange.Proto;

namespace Lagrange.Core.Message.Entities;

public sealed record GroupReaction(string FaceId, uint Type, uint Count, bool IsAdded);

public sealed class GroupReactionEntity : IMessageEntity
{
    public IReadOnlyList<GroupReaction> Reactions { get; init; } = [];
    public GroupReactionEntity() { }
    public GroupReactionEntity(IEnumerable<GroupReaction> reactions) => Reactions = reactions.ToArray();

    Elem[] IMessageEntity.Build() => [new Elem
    {
        CommonElem = new CommonElem
        {
            ServiceType = 38,
            BusinessType = 1,
            PbElem = ProtoHelper.Serialize(new GroupReactionExtra
            {
                Body = new GroupReactionBody
                {
                    Field1 = new GroupReactionHeader { Field2 = 7240 },
                    FaceInfos = Reactions.Select(x => new GroupReactionFaceInfo { FaceId = x.FaceId, Type = x.Type, Count = x.Count, IsAdded = x.IsAdded ? 1u : 0u }).ToList()
                }
            })
        }
    }];

    IMessageEntity? IMessageEntity.Parse(List<Elem> elements, Elem target)
    {
        if (target.CommonElem is not { ServiceType: 38 } common) return null;
        try
        {
            var extra = ProtoHelper.Deserialize<GroupReactionExtra>(common.PbElem.Span);
            return new GroupReactionEntity(extra.Body?.FaceInfos?.Select(x => new GroupReaction(x.FaceId ?? string.Empty, x.Type, x.Count, x.IsAdded != 0)) ?? []);
        }
        catch { return null; }
    }

    public string ToPreviewString() => "[群表情回应]";
}

[ProtoPackable]
internal partial class GroupReactionExtra { [ProtoMember(1)] public GroupReactionBody Body { get; set; } = new(); }
[ProtoPackable]
internal partial class GroupReactionBody { [ProtoMember(1)] public GroupReactionHeader Field1 { get; set; } = new(); [ProtoMember(2)] public List<GroupReactionFaceInfo> FaceInfos { get; set; } = []; }
[ProtoPackable]
internal partial class GroupReactionHeader { [ProtoMember(1)] public uint Field1 { get; set; } [ProtoMember(2)] public uint Field2 { get; set; } }
[ProtoPackable]
internal partial class GroupReactionFaceInfo { [ProtoMember(1)] public string FaceId { get; set; } = string.Empty; [ProtoMember(2)] public uint Type { get; set; } [ProtoMember(3)] public uint Count { get; set; } [ProtoMember(4)] public uint IsAdded { get; set; } }
