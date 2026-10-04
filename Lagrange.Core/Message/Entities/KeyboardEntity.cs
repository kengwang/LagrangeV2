using System.Text.Json;
using System.Text.Json.Serialization;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Utility;
using Lagrange.Proto;

namespace Lagrange.Core.Message.Entities;

/// <summary>QQ NT inline keyboard common element.</summary>
public sealed class KeyboardEntity : IMessageEntity
{
    public KeyboardData Data { get; init; } = new();

    public KeyboardEntity() { }

    public KeyboardEntity(KeyboardData data) => Data = data;

    public KeyboardEntity(string json) => Data = JsonSerializer.Deserialize(json, KeyboardJsonContext.Default.KeyboardData) ?? new();

    public string ToJson() => JsonSerializer.Serialize(Data, KeyboardJsonContext.Default.KeyboardData);

    Elem[] IMessageEntity.Build() => [new Elem
    {
        CommonElem = new CommonElem
        {
            ServiceType = 46,
            BusinessType = 1,
            PbElem = ProtoHelper.Serialize(new KeyboardExtra { Keyboard = Data })
        }
    }];

    IMessageEntity? IMessageEntity.Parse(List<Elem> elements, Elem target)
    {
        if (target.CommonElem is not { ServiceType: 46, BusinessType: 1 } common) return null;
        try { return new KeyboardEntity(ProtoHelper.Deserialize<KeyboardExtra>(common.PbElem.Span).Keyboard); }
        catch { return null; }
    }

    public string ToPreviewString() => "[Keyboard]";
}

[ProtoPackable]
internal partial class KeyboardExtra
{
    [ProtoMember(1)] public KeyboardData Keyboard { get; set; } = new();
}

[ProtoPackable]
public partial class KeyboardData
{
    [ProtoMember(1)] public List<KeyboardRow> Rows { get; set; } = [];
    [ProtoMember(2)] public ulong BotAppId { get; set; }
}

[ProtoPackable]
public partial class KeyboardRow
{
    [ProtoMember(1)] public List<KeyboardButton> Buttons { get; set; } = [];
}

[ProtoPackable]
public partial class KeyboardButton
{
    [ProtoMember(1)] public string Id { get; set; } = string.Empty;
    [ProtoMember(2)] public KeyboardRenderData RenderData { get; set; } = new();
    [ProtoMember(3)] public KeyboardAction Action { get; set; } = new();
}

[ProtoPackable]
public partial class KeyboardRenderData
{
    [ProtoMember(1)] public string Label { get; set; } = string.Empty;
    [ProtoMember(2)] public string VisitedLabel { get; set; } = string.Empty;
    [ProtoMember(3)] public uint Style { get; set; }
}

[ProtoPackable]
public partial class KeyboardAction
{
    [ProtoMember(1)] public uint Type { get; set; }
    [ProtoMember(2)] public KeyboardPermission Permission { get; set; } = new();
    [ProtoMember(3)] public uint ClickLimit { get; set; }
    [ProtoMember(6)] public bool AtBotShowChannelList { get; set; }
    [ProtoMember(9)] public uint Anchor { get; set; }
    [ProtoMember(4)] public string UnsupportedTips { get; set; } = string.Empty;
    [ProtoMember(5)] public string Data { get; set; } = string.Empty;
    [ProtoMember(7)] public bool Reply { get; set; }
    [ProtoMember(8)] public bool Enter { get; set; }
}

[ProtoPackable]
public partial class KeyboardPermission
{
    [ProtoMember(1)] public uint Type { get; set; }
    [ProtoMember(2)] public List<string> SpecifyRoleIds { get; set; } = [];
    [ProtoMember(3)] public List<string> SpecifyUserIds { get; set; } = [];
}

[JsonSerializable(typeof(KeyboardData))]
internal partial class KeyboardJsonContext : JsonSerializerContext;
