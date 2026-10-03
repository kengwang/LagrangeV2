using System.Text;
using Lagrange.Core.Internal.Packets.Message;

namespace Lagrange.Core.Message.Entities;

public sealed class XmlEntity : IMessageEntity
{
    public string Xml { get; init; } = string.Empty;

    Elem[] IMessageEntity.Build() => [new Elem { RichMsg = new RichMsg { BytesTemplate1 = Encoding.UTF8.GetBytes(Xml) } }];

    IMessageEntity? IMessageEntity.Parse(List<Elem> elements, Elem target)
    {
        if (target.RichMsg is not { } rich || rich.BytesTemplate1.IsEmpty) return null;
        return new XmlEntity { Xml = Encoding.UTF8.GetString(rich.BytesTemplate1.Span) };
    }

    public string ToPreviewString() => Xml;
}
