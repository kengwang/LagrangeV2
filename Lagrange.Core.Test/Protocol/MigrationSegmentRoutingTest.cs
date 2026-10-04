using Lagrange.Core.Common;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Events;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Services.Message;
using Lagrange.Core.Services;
using Lagrange.Core.Internal.Logic;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entities;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Test.Protocol;

public sealed class MigrationSegmentRoutingTest
{
    [Test]
    public void TemporarySendMatchesProtocolRoutingHeadAndControl()
    {
        var self = new BotFriend(1, "self", "u_self", "", "", "", null!);
        var peer = new BotStranger(2, "peer", "u_peer", "", "", 0, default, 0, null, 0, "", "", "", null);
        var message = new BotMessage([new TextEntity("reply")], self, peer, 9) { TempGroupUin = 700, ClientSequence = 7, Random = 8 };
        // Match the captured protocol fields. Core intentionally emits additional zero-valued
        // legacy content/text fields; protobuf default presence need not be byte-identical.
        var packet = ProtoHelper.Deserialize<PbSendMsgReq>(MessagePacker.Build(message).Span);
        Assert.Multiple(() =>
        {
            Assert.That(Convert.ToHexString(ProtoHelper.Serialize(packet.RoutingHead).Span), Is.EqualTo("1A0B18BC052206755F70656572"));
            Assert.That(packet.RoutingHead.C2C, Is.Null);
            Assert.That(packet.RoutingHead.Group, Is.Null);
            Assert.That(packet.ContentHead.PkgNum, Is.EqualTo(1));
            Assert.That(packet.ContentHead.PkgIndex, Is.Zero);
            Assert.That(packet.ContentHead.DivSeq, Is.EqualTo(11));
            Assert.That(packet.MessageBody.RichText.Elems.Single().Text!.TextMsg, Is.EqualTo("reply"));
            Assert.That(packet.ClientSequence, Is.EqualTo(7));
            Assert.That(packet.Random, Is.EqualTo(8));
            Assert.That(packet.Control!.MessageFlag, Is.EqualTo(9));
        });
    }

    [Test]
    public void TemporaryUploadUsesRecipientAndSameRouteAsFinalMessage()
    {
        var self = new BotFriend(1, "self", "u_self", "", "", "", null!);
        var peer = new BotStranger(2, "peer", "u_peer", "", "", 0, default, 0, null, 0, "", "", "", null);
        var message = new BotMessage([], self, peer, 9) { TempGroupUin = 700 };
        var scene = NTV2RichMedia.BuildHead(message, new ImageEntity(), 100).Scene;
        // Temporary-session image wire fixture: c2c {accountType:2,targetUid:u_peer,routingHead:grpTmp}.
        const string expected = "08021206755F706565721A0D1A0B18BC052206755F70656572";
        Assert.Multiple(() =>
        {
            Assert.That(scene.SceneType, Is.EqualTo(1));
            Assert.That(scene.Group, Is.Null);
            Assert.That(Convert.ToHexString(ProtoHelper.Serialize(scene.C2C!).Span), Is.EqualTo(expected));
            Assert.That(scene.C2C!.RoutingHead!.Value.ToArray(), Is.EqualTo(ProtoHelper.Serialize(ProtoHelper.Deserialize<PbSendMsgReq>(MessagePacker.Build(message).Span).RoutingHead).ToArray()));
        });
    }

    [Test]
    public async Task SendRejectionPreservesServerDiagnostic()
    {
        using var bot = new BotContext(new(), new(), new());
        var result = await ((IService)new SendMessageService()).Parse(Convert.FromHexString("0801120664656E696564"), bot);
        Assert.That(((SendMessageEventResp)result).ErrorMessage, Is.EqualTo("denied"));
    }

    [TestCase("0A1608011204706565722802320473656C663A040802207B1203088D01", 1, 2, 123)]
    [TestCase("0A100802120473656C6628013204706565721203088D01", 2, 1, null)]
    public async Task TemporaryPushKeepsBothIdentitiesWithoutFetchingProfiles(string hex, long sender, long receiver, long? group)
    {
        using var bot = new BotContext(new BotConfig(), new BotKeystore { Uin = 2, Uid = "self" }, new BotAppInfo());
        var packet = ProtoHelper.Deserialize<CommonMessage>(Convert.FromHexString(hex));
        var message = await new MessagePacker(bot).Parse(packet);
        Assert.Multiple(() =>
        {
            Assert.That(message.Contact.Uin, Is.EqualTo(sender));
            Assert.That(message.Receiver.Uin, Is.EqualTo(receiver));
            Assert.That(message.Contact.Uid, Is.EqualTo(sender == 2 ? "self" : "peer"));
            Assert.That(message.Receiver.Uid, Is.EqualTo(receiver == 2 ? "self" : "peer"));
            Assert.That(message.TempGroupUin, Is.EqualTo(group));
            Assert.That(message.Type, Is.EqualTo(MessageType.Temp));
        });
    }

    [Test]
    public void AnimatedFaceWirePreservesMetadataAndResult()
    {
        var element = new Elem { CommonElem = new() { ServiceType = 37, BusinessType = 1, PbElem = Convert.FromHexString("0A0170120173188102200228043201723A01784803") } };
        var entity = (FaceEntity)((IMessageEntity)new FaceEntity()).Parse([element], element)!;
        Assert.Multiple(() =>
        {
            Assert.That(entity.FaceId, Is.EqualTo(257));
            Assert.That(entity.PackId, Is.EqualTo("p"));
            Assert.That(entity.StickerId, Is.EqualTo("s"));
            Assert.That(entity.StickerType, Is.EqualTo(4));
            Assert.That(entity.ResultId, Is.EqualTo("r"));
            Assert.That(entity.SourceType, Is.EqualTo(2));
            Assert.That(entity.RandomType, Is.EqualTo(3));
            Assert.That(((IMessageEntity)entity).Build()[0].CommonElem!.PbElem.ToArray(), Is.EqualTo(element.CommonElem.PbElem.ToArray()));
        });
    }

    [TestCase("")]
    [TestCase("18FFFFFFFFFFFFFFFFFF01")]
    public void InvalidAnimatedFaceIsNotInvented(string hex)
    {
        var elem = new Elem { CommonElem = new() { ServiceType = 37, PbElem = Convert.FromHexString(hex) } };
        Assert.That(((IMessageEntity)new FaceEntity()).Parse([elem], elem), Is.Null);
    }

    [Test]
    public void SmallFacePreservesSmallSemantic()
    {
        var elem = new Elem { CommonElem = new() { ServiceType = 33, PbElem = Convert.FromHexString("088102") } };
        Assert.That(((FaceEntity)((IMessageEntity)new FaceEntity()).Parse([elem], elem)!).Large, Is.False);
    }

    [Test]
    public void FlashImageWireReadsIdentitySizeAndSubtype()
    {
        // f1 NotOnlineImage: path=f, len=3, 16-byte MD5, height=4, width=5, reserve subtype=1.
        var elem = new Elem { CommonElem = new() { ServiceType = 3, PbElem = Convert.FromHexString("0A200A016610033A10000102030405060708090A0B0C0D0E0F40044805EA01020801") } };
        var image = (ImageEntity)((IMessageEntity)new ImageEntity()).Parse([elem], elem)!;
        Assert.Multiple(() =>
        {
            Assert.That(image.IsFlash, Is.True);
            Assert.That(image.SubType, Is.EqualTo(1));
            Assert.That(image.FileUuid, Is.EqualTo("f"));
            Assert.That(image.FileSize, Is.EqualTo(3));
            Assert.That(image.ImageSize.X, Is.EqualTo(5));
            Assert.That(image.FileUrl, Does.EndWith("000102030405060708090A0B0C0D0E0F/0"));
        });
        Assert.Throws<NotSupportedException>(() => ((IMessageEntity)image).Build());
    }

    [Test]
    public void FlashCardUsesCurrentNtSchemeScene()
    {
        const string json = """{"busId":"FlashTransfer","attributes":{"scheme":"mqqrouter://flash_transfer/open_fileset?fileset_id=fs-nt&scene_type=2"}}""";
        var elem = new Elem { CommonElem = new() { ServiceType = 45, BusinessType = 3, PbElem = ProtoHelper.Serialize(new MarkdownData { Content = "[闪传](mqqapi://markdown/node?json=" + Uri.EscapeDataString(json) + ")" }) } };
        var flash = (FlashFileEntity)((IMessageEntity)new FlashFileEntity()).Parse([elem], elem)!;
        Assert.That(flash.FilesetId, Is.EqualTo("fs-nt"));
        Assert.That(flash.SceneType, Is.EqualTo(2));
    }

    [Test]
    public void ArbitraryBusinessThreeCardIsNotFlash()
    {
        var elem = new Elem { CommonElem = new() { ServiceType = 45, BusinessType = 3, PbElem = ProtoHelper.Serialize(new MarkdownData { Content = "{\"busId\":\"SomethingElse\",\"filesetId\":\"not-flash\"}" }) } };
        Assert.That(((IMessageEntity)new FlashFileEntity()).Parse([elem], elem), Is.Null);
    }

    [Test]
    public void PokeRequiresSolePrivateElement()
    {
        Assert.DoesNotThrow(() => MessagingLogic.ValidatePoke([new PokeEntity()], false));
        Assert.Throws<ArgumentException>(() => MessagingLogic.ValidatePoke([new PokeEntity()], true));
        Assert.Throws<ArgumentException>(() => MessagingLogic.ValidatePoke([new PokeEntity(), new TextEntity("x")], false));
    }
}
