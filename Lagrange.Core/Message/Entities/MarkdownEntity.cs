using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Utility;
using Lagrange.Proto;

namespace Lagrange.Core.Message.Entities;

/// <summary>QQ NT markdown common element. The raw markdown is preserved.</summary>
public sealed class MarkdownEntity : IMessageEntity
{
    public string Content { get; init; } = string.Empty;

    public MarkdownEntity() { }

    public MarkdownEntity(string content) => Content = content;

    Elem[] IMessageEntity.Build() => [new Elem
    {
        CommonElem = new CommonElem
        {
            ServiceType = 45,
            BusinessType = 1,
            PbElem = ProtoHelper.Serialize(new MarkdownData { Content = Content })
        }
    }];

    IMessageEntity? IMessageEntity.Parse(List<Elem> elements, Elem target)
    {
        if (target.CommonElem is not { ServiceType: 45 } common || common.PbElem.IsEmpty) return null;
        try { return new MarkdownEntity(ProtoHelper.Deserialize<MarkdownData>(common.PbElem.Span).Content); }
        catch { return null; }
    }

    public string ToPreviewString() => Content;
}

[ProtoPackable]
internal partial class MarkdownData
{
    [ProtoMember(1)] public string Content { get; set; } = string.Empty;
    [ProtoMember(6)] public uint ExtType { get; set; }
    [ProtoMember(7)] public FlashFileExtra? ExtInfo { get; set; }
}
