using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Internal.Packets.Notify;
using Lagrange.Core.Message.Entities;
using Lagrange.Core.Utility;
using Lagrange.Core.Message;
using System.IO.Compression;
using System.Text;

namespace Lagrange.Core.Test.Protocol;

public sealed class ProtocolMigrationTest
{
    [Test]
    public void RegistryRecognisesModernForwardBeforeGenericLightApp()
    {
        var elem = Card("""
            {"app":"com.tencent.multimsg","meta":{"detail":{"resid":"resource-123","title":"Forward","summary":"Two messages"}},"prompt":"[聊天记录]"}
            """);
        var parsed = MessageEntityRegistry.Create().Select(x => x.Parse([elem], elem)).FirstOrDefault(x => x != null);
        Assert.That(parsed, Is.TypeOf<MultiMsgEntity>());
        var forward = (MultiMsgEntity)parsed!;
        Assert.That(forward.ResId, Is.EqualTo("resource-123"));
        Assert.That(forward.Summary, Is.EqualTo("Two messages"));
    }

    [Test]
    public void RegistryPreservesUnknownLightApp()
    {
        const string json = "{\"app\":\"unknown.app\",\"extra\":\"keep me\"}";
        var elem = Card(json);
        var parsed = MessageEntityRegistry.Create().Select(x => x.Parse([elem], elem)).FirstOrDefault(x => x != null);
        Assert.That(parsed, Is.TypeOf<LightAppEntity>());
        Assert.That(((LightAppEntity)parsed!).Payload, Is.EqualTo(json));
    }

    [Test]
    public void GeneralFlagsReserveUsesFieldNineteen()
    {
        var encoded = ProtoHelper.Serialize(new GeneralFlags { PbReserve = new byte[] { 1, 2 } });
        Assert.That(Convert.ToHexString(encoded.Span), Does.EndWith("9A01020102"));
    }

    private static Elem Card(string json)
    {
        using var output = new MemoryStream();
        output.WriteByte(1);
        using (var zlib = new ZLibStream(output, CompressionLevel.Optimal, true))
            zlib.Write(Encoding.UTF8.GetBytes(json));
        return new Elem { LightAppElem = new LightAppElem { BytesData = output.ToArray() } };
    }

    [Test]
    public void FaceUsesDedicatedWireElement()
    {
        var entity = new FaceEntity { FaceId = 42, Raw = "微笑" };
        var elem = ((IMessageEntity)entity).Build().Single();
        Assert.That(elem.Face!.Index, Is.EqualTo(42));
        var parsed = ((IMessageEntity)new FaceEntity()).Parse([], elem);
        Assert.That(((FaceEntity)parsed!).FaceId, Is.EqualTo(42));
    }

    [Test]
    public void PokeRoundTripsCommonElement()
    {
        var entity = new PokeEntity(3);
        var elem = ((IMessageEntity)entity).Build().Single();
        Assert.That(elem.CommonElem!.ServiceType, Is.EqualTo(2));
        Assert.That(((PokeEntity)((IMessageEntity)new PokeEntity()).Parse([], elem)!).Type, Is.EqualTo(3));
    }

    [Test]
    public void NotifySchemasRoundTrip()
    {
        var input = new InputStatusNotify
        {
            FromUid = "uid-1",
            NotifyItem = new InputStatusNotifyItem { EventType = 3 }
        };
        var decoded = ProtoHelper.Deserialize<InputStatusNotify>(ProtoHelper.Serialize(input).Span);
        Assert.That(decoded.FromUid, Is.EqualTo("uid-1"));
        Assert.That(decoded.NotifyItem.EventType, Is.EqualTo(3));
    }

}

