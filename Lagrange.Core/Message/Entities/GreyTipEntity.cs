using System.Text.Json;
using System.Text.Json.Nodes;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Utility;
using Lagrange.Proto;

namespace Lagrange.Core.Message.Entities;

public sealed class GreyTipEntity : IMessageEntity
{
    public string GreyTip { get; init; } = string.Empty;

    public GreyTipEntity() { }

    public GreyTipEntity(string greyTip) => GreyTip = greyTip;

    Elem[] IMessageEntity.Build() =>
    [
        new Elem
        {
            GeneralFlags = new GeneralFlags
            {
                PbReserve = ProtoHelper.Serialize(new GreyTipExtra
                {
                    Layer1 = new GreyTipLayer
                    {
                        Info = new GreyTipInfo
                        {
                            Type = 1,
                            Content = new JsonObject
                            {
                                ["gray_tip"] = GreyTip,
                                ["object_type"] = 3,
                                ["sub_type"] = 2,
                                ["type"] = 4
                            }.ToJsonString()
                        }
                    }
                })
            }
        }
    ];

    IMessageEntity? IMessageEntity.Parse(List<Elem> elements, Elem target)
    {
        if (target.GeneralFlags is not { } flags || flags.PbReserve.IsEmpty) return null;
        var extra = ProtoHelper.Deserialize<GreyTipExtra>(flags.PbReserve.Span);
        if (extra.Layer1?.Info is not { Type: 1 } info) return null;
        using var json = JsonDocument.Parse(info.Content);
        return json.RootElement.TryGetProperty("gray_tip", out var text) && text.ValueKind == JsonValueKind.String
            ? new GreyTipEntity(text.GetString()!)
            : null;
    }

    public string ToPreviewString() => GreyTip;
}

[ProtoPackable]
internal partial class GreyTipExtra
{
    [ProtoMember(101)] public GreyTipLayer? Layer1 { get; set; }
}

[ProtoPackable]
internal partial class GreyTipLayer
{
    [ProtoMember(1)] public GreyTipInfo? Info { get; set; }
}

[ProtoPackable]
internal partial class GreyTipInfo
{
    [ProtoMember(1)] public uint Type { get; set; }
    [ProtoMember(2)] public string Content { get; set; } = string.Empty;
}
