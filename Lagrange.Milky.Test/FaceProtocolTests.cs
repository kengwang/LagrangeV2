using System.Text.Json;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entities;
using Lagrange.Milky.Caching;
using Lagrange.Milky.Converters;
using Lagrange.Milky.Models.Segments;
using Lagrange_Milky;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lagrange.Milky.Test;

public class FaceProtocolTests
{
    [Test]
    public void OutgoingFaceDefaultsToNormalFace()
    {
        var options = new JsonSerializerOptions().AddSerializerContextsFromLagrange_Milky();
        var segment = (FaceOutgoingSegment)JsonSerializer.Deserialize(
            """{"type":"face","data":{"face_id":"123"}}""",
            options.GetTypeInfo(typeof(OutgoingSegmentBase)))!;
        Assert.That(segment.Data.Large, Is.False);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task IncomingFaceUsesProtocolStringAndIsLarge(bool large)
    {
        var converter = new MilkyConverter(null!, new MessageCache(null!, NullLoggerFactory.Instance), null!);
        MessageChain chain = [new FaceEntity { FaceId = 123, Large = large }];
        var segments = await converter.ToIncomingSegmentsAsync(chain, MessageType.Private, 123);
        var options = new JsonSerializerOptions().AddSerializerContextsFromLagrange_Milky();
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(segments.Single(), options.GetTypeInfo(typeof(IncomingSegmentBase))));
        var data = json.RootElement.GetProperty("data");
        Assert.That(data.GetProperty("face_id").ValueKind, Is.EqualTo(JsonValueKind.String));
        Assert.That(data.GetProperty("face_id").GetString(), Is.EqualTo("123"));
        Assert.That(data.GetProperty("is_large").GetBoolean(), Is.EqualTo(large));
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task OutgoingFaceAcceptsProtocolStringAndIsLarge(bool large)
    {
        var options = new JsonSerializerOptions().AddSerializerContextsFromLagrange_Milky();
        string json = "{\"type\":\"face\",\"data\":{\"face_id\":\"123\",\"is_large\":" + (large ? "true" : "false") + "}}";
        var segment = (OutgoingSegmentBase)JsonSerializer.Deserialize(json, options.GetTypeInfo(typeof(OutgoingSegmentBase)))!;
        var converter = new MilkyConverter(null!, new MessageCache(null!, NullLoggerFactory.Instance), null!);
        var entities = await converter.FromOutgoingSegmentsAsync([segment], MessageType.Private, 123, CancellationToken.None);
        var face = (FaceEntity)entities.Single();
        Assert.That(face.FaceId, Is.EqualTo(123));
        Assert.That(face.Large, Is.EqualTo(large));
    }
}
