using Lagrange.Core.Common.Entity;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entities;
using Lagrange.Core.Utility;
using Lagrange.Testing;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Services.Message;
using Lagrange.Core.Services;

namespace Lagrange.Core.Test.Protocol;

public sealed class MessageStyleTests
{
    private static BotMessage Message(MessageStyle? style) => BotMessage.CreateCustomFriend(
        100, "sender", 200, "receiver", 123, new MessageBuilder().Text("body").Style(style).Build());

    private static void AssertStyle(MessageStyle? actual, MessageStyle expected)
    {
        Assert.That(actual, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(actual!.BubbleId, Is.EqualTo(expected.BubbleId));
            Assert.That(actual.PendantId, Is.EqualTo(expected.PendantId));
            Assert.That(actual.FontId, Is.EqualTo(expected.FontId));
            Assert.That(actual.FontEffectId, Is.EqualTo(expected.FontEffectId));
            Assert.That(actual.IsCsFontEffectEnabled, Is.EqualTo(expected.IsCsFontEffectEnabled));
            Assert.That(actual.BubbleDiyTextId, Is.EqualTo(expected.BubbleDiyTextId));
        });
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task SendAndForwardRoundTripBoundaryValues(bool enabled)
    {
        using var replay = new MessageProtocolReplay(_ => throw new InvalidOperationException("Unexpected request"));
        var packer = new MessagePacker(replay.Bot);
        foreach (var font in new ushort[] { 0, 0x1234, ushort.MaxValue })
        {
            var style = new MessageStyle { BubbleId = ulong.MaxValue, PendantId = ulong.MaxValue,
                FontId = font, FontEffectId = uint.MaxValue, BubbleDiyTextId = uint.MaxValue,
                IsCsFontEffectEnabled = enabled };
            var message = Message(style);
            Assert.That(message.Style, Is.SameAs(message.Entities.Style));
            var sent = ProtoHelper.Deserialize<PbSendMsgReq>(MessagePacker.Build(message).Span);
            var extra = ProtoHelper.Deserialize<MessageStyleExtra>(sent.MessageBody.RichText.Elems.Last().GeneralFlags!.PbReserve.Span);
            ulong expectedFont = font == 0 ? 0x10000UL : (ulong)((font >> 8) | ((font << 8) & 0xFFFF)) | 0x20000UL;
            Assert.That(extra.Font, Is.EqualTo(expectedFont | (enabled ? 1UL << 24 : 0)));
            var node = await packer.BuildFake(message);
            Assert.That(ProtoHelper.Serialize(sent.MessageBody).ToArray(), Is.EqualTo(ProtoHelper.Serialize(node.MessageBody!).ToArray()));
            var parsed = await packer.ParseForwarded(ProtoHelper.Deserialize<CommonMessage>(ProtoHelper.Serialize(node).Span));
            AssertStyle(parsed.Style, style);
            Assert.That(parsed.Entities.Single(), Is.TypeOf<TextEntity>());
        }
    }

    [Test]
    public void WireTagsAndFontBitsMatchIndependentBytes()
    {
        // Elem field 9 -> nested field 1 = bubble 7.
        Assert.That(ProtoHelper.Serialize(new Elem { ElemFlags2 = new() { ColorTextId = 7 } }).ToArray(),
            Is.EqualTo(new byte[] { 0x4A, 2, 8, 7 }));
        // GeneralFlags field 1 = diy 9; field 17 = pendant 8.
        var flags = ProtoHelper.Deserialize<GeneralFlags>(new byte[] { 8, 9, 0x88, 1, 8 });
        Assert.That(flags.BubbleDiyTextId, Is.EqualTo(9));
        Assert.That(flags.PendantId, Is.EqualTo(8));
        Assert.That(ProtoHelper.Serialize(new Elem { GeneralFlags = flags }).ToArray().Take(2),
            Is.EqualTo(new byte[] { 0xAA, 2 }));
        // Reserve field 15 = 0x01023412, field 34 = 2000.
        var extra = ProtoHelper.Deserialize<MessageStyleExtra>(new byte[] { 0x78, 0x92, 0xE8, 0x88, 8, 0x90, 2, 0xD0, 0x0F });
        Assert.That(extra.Font, Is.EqualTo(0x01023412UL));
        Assert.That(extra.FontEffectId, Is.EqualTo(2000));
    }

    [Test]
    public async Task AbsentAndExplicitZeroStyleRemainDistinct()
    {
        using var replay = new MessageProtocolReplay(_ => throw new InvalidOperationException());
        var packer = new MessagePacker(replay.Bot);
        var plain = Message(null);
        Assert.That((await packer.ParseForwarded(await packer.BuildFake(plain))).Style, Is.Null);
        Assert.That(ProtoHelper.Deserialize<PbSendMsgReq>(MessagePacker.Build(plain).Span).MessageBody.RichText.Elems, Has.Count.EqualTo(1));
        AssertStyle((await packer.ParseForwarded(await packer.BuildFake(Message(new())))).Style, new());
        Assert.That(new MessageBuilder().Style(new()).Style(null).Build().Style, Is.Null);
    }

    [Test]
    public async Task IndependentMetadataMergesWithoutBeingClearedByOtherFlags()
    {
        using var replay = new MessageProtocolReplay(_ => throw new InvalidOperationException());
        var node = MessageProtocolReplay.ForwardNode();
        var elems = node.MessageBody!.RichText.Elems;
        elems.Add(ProtoHelper.Deserialize<Elem>(new byte[] { 0x4A, 2, 8, 7 }));
        elems.Add(new() { GeneralFlags = new() { PendantId = 8, BubbleDiyTextId = 9 } });
        elems.Add(new() { GeneralFlags = new() { PbReserve = new byte[] { 0x78, 0x92, 0xE8, 0x88, 8, 0x90, 2, 0xD0, 0x0F } } });
        elems.AddRange(((IMessageEntity)new GreyTipEntity("tip")).Build());
        elems.AddRange(((IMessageEntity)new StreamEntity("stream")).Build());
        elems.AddRange(((IMessageEntity)new LongMsgEntity()).Build());
        elems.Add(new() { GeneralFlags = new() { PbReserve = new byte[] { 0x78, 0x80 } } });
        var parsed = await new MessagePacker(replay.Bot).ParseForwarded(node);
        AssertStyle(parsed.Style, new() { BubbleId = 7, PendantId = 8, BubbleDiyTextId = 9,
            FontId = 0x1234, FontEffectId = 2000, IsCsFontEffectEnabled = true });
        Assert.That(parsed.Entities.OfType<GreyTipEntity>().Single().GreyTip, Is.EqualTo("tip"));
        Assert.That(parsed.Entities.OfType<StreamEntity>().Count(), Is.EqualTo(1));
        Assert.That(parsed.Entities.OfType<LongMsgEntity>().Count(), Is.EqualTo(1));
        node.MessageBody.RichText.Elems = elems.Skip(4).ToList();
        Assert.That((await new MessagePacker(replay.Bot).ParseForwarded(node)).Style, Is.Null);
    }

    [Test]
    public async Task LegacyVoiceStillParsesStyle()
    {
        using var replay = new MessageProtocolReplay(_ => throw new InvalidOperationException());
        var packer = new MessagePacker(replay.Bot);
        var node = await packer.BuildFake(Message(new() { BubbleId = 7 }));
        node.MessageBody!.RichText.Ptt = new Ptt { FileName = "voice", FileMd5 = [], FileUuid = [] };
        var parsed = await packer.ParseForwarded(node);
        Assert.That(parsed.Style!.BubbleId, Is.EqualTo(7));
        Assert.That(parsed.Entities.Single(), Is.TypeOf<RecordEntity>());
    }

    [Test]
    public async Task NestedForwardUploadAndDownloadRetainNodeStyles()
    {
        using var replay = new MessageProtocolReplay(_ => ProtoHelper.Serialize(
            new LongMsgInterfaceRsp { SendRsp = new() { ResId = "nested-style" } }));
        var leaf = Message(new() { PendantId = 8 });
        var plain = Message(null);
        var root = BotMessage.CreateCustomFriend(100, "root", 200, "self", 123,
            new MessageBuilder().MultiMsg([leaf, plain]).Style(new() { BubbleId = 7 }).Build());
        var content = await new ForwardedMessageBuilder(replay.Bot,
            new BotGroup(400, "group", 0, 0, 0, null, null, null)).Build([root]);
        var parsed = (LongMsgRecvEventResp)await ((IService)new LongMsgRecvService())
            .Parse(NestedForwardTest.Response(content), replay.Bot);
        Assert.That(parsed.Messages.Single().Style!.BubbleId, Is.EqualTo(7));
        var children = ((MultiMsgEntity)parsed.Messages.Single().Entities.Single()).Messages;
        Assert.That(children[0].Style!.PendantId, Is.EqualTo(8));
        Assert.That(children[0].Style!.BubbleId, Is.Zero);
        Assert.That(children[1].Style, Is.Null);
        Assert.That(replay.Requests, Has.Count.EqualTo(1));
    }

    [TestCase(166)]
    [TestCase(82)]
    [TestCase(141)]
    public async Task IncomingPrivateGroupAndTempPathsRetainStyle(int type)
    {
        using var replay = new MessageProtocolReplay(_ => throw new InvalidOperationException("Unexpected request"));
        var packer = new MessagePacker(replay.Bot);
        var source = type == 82
            ? BotMessage.CreateCustomGroup(400, 100, "sender", 123, new MessageBuilder().Text("body").Style(new() { BubbleId = 7 }).Build())
            : Message(new() { BubbleId = 7 });
        var node = await packer.BuildFake(source);
        node.ContentHead.Type = type;
        node.RoutingHead.ToUin = 200;
        if (source.Contact is BotGroupMember member)
            replay.SeedContacts(member.Group, member, new BotGroupMember(member.Group, 200, "u_self", "self", GroupMemberPermission.Member, 0, null, null, 0, 0, 0));
        else replay.SeedContacts(source.Contact, source.Receiver);
        if (type == 141) node.RoutingHead.CommonC2C.FromTinyId = 400;
        var parsed = await packer.Parse(ProtoHelper.Deserialize<CommonMessage>(ProtoHelper.Serialize(node).Span));
        Assert.That(parsed.Style!.BubbleId, Is.EqualTo(7));
        Assert.That(parsed.Type, Is.EqualTo(type == 82 ? MessageType.Group : type == 141 ? MessageType.Temp : MessageType.Private));
        Assert.That(replay.Requests, Is.Empty);
    }
}
