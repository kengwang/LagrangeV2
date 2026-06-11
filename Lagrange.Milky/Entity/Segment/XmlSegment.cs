using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity.Segment;

[method: JsonConstructor]
public class XmlIncomingSegment(XmlSegmentData data) : IncomingSegmentBase<XmlSegmentData>(data)
{
    public XmlIncomingSegment(int serviceId, string xmlPayload) : this(new XmlSegmentData(serviceId, xmlPayload)) { }
}

public class XmlSegmentData(int serviceId, string xmlPayload)
{
    [JsonPropertyName("service_id")]
    public int ServiceId { get; } = serviceId;

    [JsonPropertyName("xml_payload")]
    public string XmlPayload { get; } = xmlPayload;
}
