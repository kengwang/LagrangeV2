using System.IO.Compression;
using System.Text;
using System.Text.Json.Nodes;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Internal.Services.Message;
using Lagrange.Core.Message.Entities;
using Lagrange.Core.Message;
using Lagrange.Core.Services;
using Lagrange.Core.Utility;
using Lagrange.Testing;

namespace Lagrange.Core.Test.Protocol;

public sealed class NestedForwardTest
{
    // Minimized message 1289: two root nodes, and a named embedded item with
    // texts 123/456. Identities and external resource IDs are synthetic.
    internal static byte[] CapturedShape()
    {
        var root = MessageProtocolReplay.ForwardNode();
        const string xml = "<msg serviceID=\"35\" action=\"viewMultiMsg\" m_fileName=\"embedded-child\" m_resid=\"unavailable\" tSum=\"2\"><item><title>Historical title</title><title>sender: 123</title><summary>two messages</summary></item></msg>";
        using var stream = new MemoryStream();
        stream.WriteByte(1);
        using (var zlib = new ZLibStream(stream, CompressionLevel.Optimal, true)) zlib.Write(Encoding.UTF8.GetBytes(xml));
        root.MessageBody!.RichText.Elems = [new Elem { RichMsg = new RichMsg { ServiceId = 35, BytesTemplate1 = stream.ToArray() } }];
        var sibling = MessageProtocolReplay.ForwardNode();
        sibling.MessageBody!.RichText.Elems = [new Elem { Text = new Text { TextMsg = "456" } }];
        var child = MessageProtocolReplay.ForwardNode();
        child.MessageBody!.RichText.Elems = [new Elem { Text = new Text { TextMsg = "123" } }];
        return Response(new PbMultiMsgTransmit { Items =
        [
            new PbMultiMsgItem { FileName = "MultiMsg", Buffer = new() { Msg = [root, sibling] } },
            new PbMultiMsgItem { FileName = "embedded-child", Buffer = new() { Msg = [child, sibling] } }
        ] });
    }

    internal static byte[] Response(PbMultiMsgTransmit content)
    {
        using var stream = new MemoryStream();
        using (var gzip = new GZipStream(stream, CompressionLevel.Optimal, true)) gzip.Write(ProtoHelper.Serialize(content).Span);
        return ProtoHelper.Serialize(new LongMsgInterfaceRsp { RecvRsp = new() { Payload = stream.ToArray(), ResId = "root" } }).ToArray();
    }

    [Test]
    public async Task EmbeddedItemMustResolveWithoutFetchingExternalResource()
    {
        using var replay = new MessageProtocolReplay(_ => throw new AssertionException("Embedded child must not be downloaded."));
        var result = (LongMsgRecvEventResp)await ((IService)new LongMsgRecvService()).Parse(CapturedShape(), replay.Bot);
        Assert.That(result.Messages, Has.Count.EqualTo(2));
        var child = (MultiMsgEntity)result.Messages[0].Entities.Single();
        Assert.That(child.Messages.Select(x => ((TextEntity)x.Entities.Single()).Text), Is.EqualTo(new[] { "123", "456" }));
        Assert.That(replay.Requests, Is.Empty);
        Assert.That(child.MessagesLoaded, Is.True);
        Assert.That(child.FileName, Is.EqualTo("embedded-child"));
        Assert.That(child.Title, Is.EqualTo("Historical title"));
        Assert.That(child.Preview, Is.EqualTo(new[] { "sender: 123" }));
        Assert.That(child.Summary, Is.EqualTo("two messages"));
    }

    internal static PbMultiMsgTransmit Unpack(ReadOnlyMemory<byte> request)
    {
        var packet = ProtoHelper.Deserialize<LongMsgInterfaceReq>(request.Span);
        using var source = new MemoryStream(packet.SendReq.Payload);
        using var gzip = new GZipStream(source, CompressionMode.Decompress);
        using var dest = new MemoryStream();
        gzip.CopyTo(dest);
        return ProtoHelper.Deserialize<PbMultiMsgTransmit>(dest.ToArray());
    }

    internal static JsonNode Card(Elem elem)
    {
        using var source = new MemoryStream(elem.LightAppElem!.BytesData.ToArray()[1..]);
        using var zlib = new ZLibStream(source, CompressionMode.Decompress);
        using var reader = new StreamReader(zlib);
        return JsonNode.Parse(reader.ReadToEnd())!;
    }

    private static BotMessage Node(params IMessageEntity[] entities) => BotMessage.CreateCustomFriend(100, "sender", 0, "", 123, [.. entities]);

    [TestCase(2, false)]
    [TestCase(2, true)]
    [TestCase(3, false)]
    public async Task BuilderUploadsBottomUpAndEmbedsAllDescendants(int depth, bool markdown)
    {
        var uploads = new List<PbMultiMsgTransmit>();
        using var replay = new MessageProtocolReplay(request =>
        {
            var packet = ProtoHelper.Deserialize<LongMsgInterfaceReq>(request.Data.Span);
            Assert.That(packet.SendReq.GroupUin, Is.EqualTo(400));
            Assert.That(packet.SendReq.PeerInfo!.PeerUid, Is.EqualTo("400"));
            var content = Unpack(request.Data);
            AssertReferences(content);
            uploads.Add(content);
            return ProtoHelper.Serialize(new LongMsgInterfaceRsp { SendRsp = new() { ResId = $"resource-{uploads.Count}" } });
        });
        var messages = new List<BotMessage> { Node(markdown ? new MarkdownEntity("**markdown**") : new TextEntity("text")) };
        for (int i = 1; i < depth; i++) messages = [Node(new MultiMsgEntity(messages) { Title = "title", Preview = ["preview"], Summary = "summary", Prompt = "prompt" })];
        var group = new BotGroup(400, "group", 0, 0, 0, null, null, null);
        var built = await new ForwardedMessageBuilder(replay.Bot, group).Build(messages);
        Assert.That(uploads, Has.Count.EqualTo(depth - 1));
        Assert.That(built.Items, Has.Count.EqualTo(depth));
        AssertReferences(built);
        foreach (var node in built.Items.SelectMany(x => x.Buffer.Msg))
        {
            Assert.That(node.ContentHead.Type, Is.EqualTo(9));
            Assert.That(node.RoutingHead.CommonC2C.Name, Is.EqualTo("sender"));
            foreach (var elem in node.MessageBody!.RichText.Elems.Where(x => x.LightAppElem is not null))
            {
                var card = Card(elem);
                Assert.That(card["meta"]!["detail"]!["title"]!.ToString(), Is.EqualTo("title"));
                Assert.That(card["meta"]!["detail"]!["summary"]!.ToString(), Is.EqualTo("summary"));
                Assert.That(card["prompt"]!.ToString(), Is.EqualTo("prompt"));
            }
        }
        var parsed = (LongMsgRecvEventResp)await ((IService)new LongMsgRecvService()).Parse(Response(built), replay.Bot);
        var leaf = parsed.Messages.Single();
        for (int i = 1; i < depth; i++) leaf = ((MultiMsgEntity)leaf.Entities.Single()).Messages.Single();
        Assert.That(leaf.Entities.Single(), markdown ? Is.TypeOf<MarkdownEntity>() : Is.TypeOf<TextEntity>());
        Assert.That(replay.Requests, Has.Count.EqualTo(depth - 1));
    }

    internal static void AssertReferences(PbMultiMsgTransmit content)
    {
        var names = content.Items.Select(x => x.FileName).ToArray();
        Assert.That(names.Distinct().Count(), Is.EqualTo(names.Length));
        foreach (var elem in content.Items.SelectMany(x => x.Buffer.Msg).SelectMany(x => x.MessageBody!.RichText.Elems).Where(x => x.LightAppElem is not null))
        {
            var card = Card(elem);
            var name = JsonNode.Parse(card["extra"]!.ToString())!["filename"]!.ToString();
            Assert.That(names, Does.Contain(name));
            Assert.That(card["meta"]!["detail"]!["uniseq"]!.ToString(), Is.EqualTo(name));
        }
    }

    [Test]
    public async Task ReforwardRemapsNamesAndReusesSharedChild()
    {
        using var replay = new MessageProtocolReplay(request => ProtoHelper.Serialize(new LongMsgInterfaceRsp { SendRsp = new() { ResId = "uploaded" } }));
        var received = (LongMsgRecvEventResp)await ((IService)new LongMsgRecvService()).Parse(CapturedShape(), replay.Bot);
        var first = (MultiMsgEntity)received.Messages[0].Entities.Single();
        var second = new MultiMsgEntity([Node(new TextEntity("different"))]) { FileName = first.FileName };
        var built = await new ForwardedMessageBuilder(replay.Bot, new BotGroup(400, "", 0, 0, 0, null, null, null))
            .Build([Node(first, second, first)]);
        Assert.That(built.Items, Has.Count.EqualTo(3));
        Assert.That(replay.Requests, Has.Count.EqualTo(2));
        AssertReferences(built);
        var cards = built.Items[0].Buffer.Msg.Single().MessageBody!.RichText.Elems.Select(Card).ToArray();
        Assert.That(cards[0]["extra"]!.ToString(), Is.EqualTo(cards[2]["extra"]!.ToString()));
        Assert.That(cards[0]["extra"]!.ToString(), Is.Not.EqualTo(cards[1]["extra"]!.ToString()));
    }

    [Test]
    public void CycleFailsBeforeAnyUpload()
    {
        using var replay = new MessageProtocolReplay(_ => throw new AssertionException("Must not upload."));
        var cyclic = new MultiMsgEntity();
        cyclic.Messages.Add(Node(cyclic));
        Assert.ThrowsAsync<OperationException>(() => new ForwardedMessageBuilder(replay.Bot, new BotFriend(200, "", "", "", "", "", null!)).Build([Node(cyclic)]));
        Assert.That(replay.Requests, Is.Empty);
    }

    [Test]
    public async Task EmptyEmbeddedChildIsLoadedAndDoesNotDownloadOnReforward()
    {
        using var replay = new MessageProtocolReplay(request => ProtoHelper.Serialize(new LongMsgInterfaceRsp { SendRsp = new() { ResId = "empty-upload" } }));
        var node = MessageProtocolReplay.ForwardNode();
        var child = new MultiMsgEntity("unavailable") { FileName = "empty" };
        node.MessageBody!.RichText.Elems = ((IMessageEntity)child).Build().ToList();
        var response = Response(new PbMultiMsgTransmit { Items =
        [new() { FileName = "MultiMsg", Buffer = new() { Msg = [node] } }, new() { FileName = "empty", Buffer = new() { Msg = [] } }] });
        var parsed = (LongMsgRecvEventResp)await ((IService)new LongMsgRecvService()).Parse(response, replay.Bot);
        var result = (MultiMsgEntity)parsed.Messages.Single().Entities.Single();
        Assert.That(result.MessagesLoaded, Is.True);
        Assert.That(result.Messages, Is.Empty);
        await new ForwardedMessageBuilder(replay.Bot, new BotFriend(200, "", "", "", "", "", null!)).Build(parsed.Messages);
        Assert.That(replay.Requests.Single().Command, Does.EndWith("SsoSendLongMsg"));
    }

    [Test]
    public void StableCardNamesAndLightAppFilenamePrecedence()
    {
        var entity = new MultiMsgEntity("resource");
        var first = Card(((IMessageEntity)entity).Build().Single());
        var second = Card(((IMessageEntity)entity).Build().Single());
        Assert.That(first["extra"]!.ToString(), Is.EqualTo(second["extra"]!.ToString()));
        var card = entity.BuildCard("resource", "named", 0).Single();
        var parsed = (MultiMsgEntity)((IMessageEntity)new MultiMsgEntity()).Parse([card], card)!;
        Assert.That(parsed.FileName, Is.EqualTo("named"));
        var withoutExtra = first.DeepClone();
        withoutExtra.AsObject().Remove("extra");
        var fallbackElem = JsonCard(withoutExtra);
        var fallback = (MultiMsgEntity)((IMessageEntity)new MultiMsgEntity()).Parse([fallbackElem], fallbackElem)!;
        Assert.That(fallback.FileName, Is.EqualTo(entity.FileName));
        var conflicting = first.DeepClone();
        conflicting["meta"]!["detail"]!["uniseq"] = "wrong";
        var conflictingElem = JsonCard(conflicting);
        var preferred = (MultiMsgEntity)((IMessageEntity)new MultiMsgEntity()).Parse([conflictingElem], conflictingElem)!;
        Assert.That(preferred.FileName, Is.EqualTo(entity.FileName));
    }

    private static Elem JsonCard(JsonNode json)
    {
        using var stream = new MemoryStream();
        stream.WriteByte(1);
        using (var zlib = new ZLibStream(stream, CompressionLevel.Optimal, true)) zlib.Write(Encoding.UTF8.GetBytes(json.ToJsonString()));
        return new Elem { LightAppElem = new() { BytesData = stream.ToArray() } };
    }

    [Test]
    public async Task FakeNodeKeepsSnapshotUidAndAllIdentityFields()
    {
        using var replay = new MessageProtocolReplay(_ => throw new AssertionException("No profile fetch."));
        var source = new BotFriend(100, "historical", "snapshot_uid", "", "", "", null!);
        var receiver = new BotFriend(200, "recipient", "recipient_uid", "", "", "", null!);
        var msg = new BotMessage(source, receiver, 123) { Random = ulong.MaxValue, Sequence = 456, ClientSequence = 789, MessageId = 10 };
        var node = await new MessagePacker(replay.Bot).BuildFake(msg);
        Assert.Multiple(() =>
        {
            Assert.That(node.RoutingHead.FromUid, Is.EqualTo("snapshot_uid"));
            Assert.That(node.RoutingHead.ToUid, Is.EqualTo("recipient_uid"));
            Assert.That(node.ContentHead.Type, Is.EqualTo(9));
            Assert.That(node.ContentHead.Random, Is.EqualTo(ulong.MaxValue));
            Assert.That(node.ContentHead.Sequence, Is.EqualTo(456));
            Assert.That(node.ContentHead.ClientSequence, Is.EqualTo(789));
            Assert.That(node.ContentHead.MsgUid, Is.EqualTo(10));
            Assert.That(node.ContentHead.Time, Is.EqualTo(123));
            Assert.That(replay.Requests, Is.Empty);
        });
    }

    [TestCase(false)]
    [TestCase(true)]
    public void MissingRootOrDuplicateItemIsExplicitError(bool duplicate)
    {
        using var replay = new MessageProtocolReplay(_ => throw new AssertionException("Must not download."));
        var item = new PbMultiMsgItem { FileName = duplicate ? "MultiMsg" : "other", Buffer = new() { Msg = [] } };
        var content = new PbMultiMsgTransmit { Items = duplicate ? [item, item] : [item] };
        Assert.ThrowsAsync<OperationException>(async () => await ((IService)new LongMsgRecvService()).Parse(Response(content), replay.Bot));
    }

    [Test]
    public void EmbeddedCycleIsExplicitError()
    {
        using var replay = new MessageProtocolReplay(_ => throw new AssertionException("Must not download."));
        var node = MessageProtocolReplay.ForwardNode();
        node.MessageBody!.RichText.Elems = new MultiMsgEntity("unused").BuildCard("unused", "MultiMsg", 1).ToList();
        var content = new PbMultiMsgTransmit { Items = [new() { FileName = "MultiMsg", Buffer = new() { Msg = [node] } }] };
        Assert.ThrowsAsync<OperationException>(async () => await ((IService)new LongMsgRecvService()).Parse(Response(content), replay.Bot));
    }

    [Test]
    public async Task ExternalFallbackIsDownloadedOnceForRepeatedResource()
    {
        using var replay = new MessageProtocolReplay(request =>
        {
            Assert.That(request.Command, Is.EqualTo(MessageProtocolReplay.LongMessageCommand));
            return MessageProtocolReplay.LongResponse(MessageProtocolReplay.ForwardNode());
        });
        var first = MessageProtocolReplay.ForwardNode("external");
        var second = MessageProtocolReplay.ForwardNode("external");
        var parsed = (LongMsgRecvEventResp)await ((IService)new LongMsgRecvService())
            .Parse(MessageProtocolReplay.LongResponse(first, second), replay.Bot);
        Assert.That(replay.Requests, Has.Count.EqualTo(1));
        Assert.That(parsed.Messages.Select(x => ((MultiMsgEntity)x.Entities.Single()).Messages.Single().Entities.Single()), Has.All.TypeOf<TextEntity>());
    }

    [Test]
    public void ExternalResourceCycleAndDownloadFailureStayVisible()
    {
        using var cyclic = new MessageProtocolReplay(_ => MessageProtocolReplay.LongResponse(MessageProtocolReplay.ForwardNode("same")));
        Assert.ThrowsAsync<OperationException>(async () => await ForwardedMessageResolver.Load(cyclic.Bot, false, "same"));
        Assert.That(cyclic.Requests, Has.Count.EqualTo(1));
        using var failing = new MessageProtocolReplay(_ => throw new IOException("offline"));
        var entity = new MultiMsgEntity("external");
        Assert.CatchAsync<Exception>(() => new ForwardedMessageBuilder(failing.Bot, new BotFriend(200, "", "", "", "", "", null!)).Build([Node(entity)]));
        Assert.That(failing.Requests.Single().Command, Is.EqualTo(MessageProtocolReplay.LongMessageCommand));
    }
}
