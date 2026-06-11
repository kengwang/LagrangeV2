using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Utility;
using Lagrange.Proto;

namespace Lagrange.Core.Message.Entities;

public class MarkdownEntity(string content) : IMessageEntity
{
    public string Content { get; set; } = content;

    public MarkdownEntity() : this(string.Empty) { }

    Elem[] IMessageEntity.Build()
    {
        return
        [
            new Elem
            {
                CommonElem = new CommonElem
                {
                    ServiceType = 45,
                    PbElem = ProtoHelper.Serialize(new MarkdownData { Content = Content }),
                    BusinessType = 1
                }
            }
        ];
    }

    IMessageEntity? IMessageEntity.Parse(List<Elem> elements, Elem target)
    {
        if (target.CommonElem is not { ServiceType: 45, BusinessType: 1, PbElem.IsEmpty: false } commonElem)
        {
            return null;
        }

        var data = ProtoHelper.Deserialize<MarkdownData>(commonElem.PbElem.Span);
        return new MarkdownEntity(data.Content);
    }

    string IMessageEntity.ToPreviewString() => $"[Markdown] {Content}";
}

[ProtoPackable]
internal partial class MarkdownData
{
    [ProtoMember(1)] public string Content { get; set; } = string.Empty;
}
