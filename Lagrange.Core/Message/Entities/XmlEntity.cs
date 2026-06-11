using System.Text;
using System.Xml;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Utility.Compression;

namespace Lagrange.Core.Message.Entities;

public class XmlEntity(string xml, int serviceId) : IMessageEntity
{
    private static readonly byte[] Header = [0x01];

    public string Xml { get; set; } = xml;

    public int ServiceId { get; set; } = serviceId;

    public XmlEntity() : this(string.Empty, 35) { }

    public XmlEntity(string xml) : this(xml, 35) { }

    Elem[] IMessageEntity.Build()
    {
        return
        [
            new Elem
            {
                RichMsg = new RichMsg
                {
                    ServiceId = (uint)ServiceId,
                    BytesTemplate1 = ZCompression.ZCompress(Xml, Header)
                }
            }
        ];
    }

    IMessageEntity? IMessageEntity.Parse(List<Elem> elements, Elem target)
    {
        if (target.RichMsg is not { BytesTemplate1.IsEmpty: false } richMsg) return null;

        var compressed = richMsg.BytesTemplate1.Span;
        if (compressed.Length <= Header.Length) return null;

        var xml = ZCompression.ZDecompress(compressed[Header.Length..]);
        string xmlPayload = Encoding.UTF8.GetString(xml);
        if (IsMultiMsgXml(xmlPayload)) return null;

        return new XmlEntity(xmlPayload, (int)richMsg.ServiceId);
    }

    string IMessageEntity.ToPreviewString() => "[XML]";

    private static bool IsMultiMsgXml(string xml)
    {
        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            return doc["msg"]?.Attributes["m_resid"] != null;
        }
        catch (XmlException)
        {
            return false;
        }
    }
}
