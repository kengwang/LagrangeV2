using Lagrange.Core.Common;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Internal.Logic;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Internal.Services.Message;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entities;
using Lagrange.Core.Services;
using Lagrange.Core.Utility;
using Lagrange.Testing;

namespace Lagrange.Core.Test.Protocol;

public sealed class MilkyErrorRegressionTest
{
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public async Task CapturedNotificationPreservesWideRandomAndFollowingFields(int index)
    {
        using var fixtures = MilkyLogFixture.Read();
        var fixture = fixtures.RootElement.GetProperty("notifications")[index];
        using var bot = new BotContext(new(), new(), new());
        var push = (PushMessageEvent)await ((IService)new PushMessageService()).Parse(MilkyLogFixture.Bytes(fixture), bot);
        var head = push.MsgPush.CommonMessage.ContentHead;
        Assert.Multiple(() =>
        {
            Assert.That(head.Type, Is.EqualTo(732));
            Assert.That(head.SubType, Is.EqualTo(20));
            Assert.That(head.Random, Is.EqualTo(fixture.GetProperty("random").GetUInt64()));
            Assert.That(head.Sequence, Is.EqualTo(fixture.GetProperty("sequence").GetUInt64()));
            Assert.That(head.Time, Is.EqualTo(fixture.GetProperty("time").GetInt64()));
            Assert.That(head.MsgUid, Is.EqualTo(fixture.GetProperty("msgUid").GetUInt64()));
        });
    }

    [TestCase(0, 25)]
    [TestCase(1, 6)]
    [TestCase(2, 25)]
    public async Task CapturedForwardedResponsePreservesEveryNode(int index, int count)
    {
        using var fixtures = MilkyLogFixture.Read();
        var fixture = fixtures.RootElement.GetProperty("forwards")[index];
        using var bot = new BotContext(new(), new(), new());
        var result = (LongMsgRecvEventResp)await ((IService)new LongMsgRecvService()).Parse(MilkyLogFixture.Bytes(fixture), bot);
        Assert.That(result.Messages, Has.Count.EqualTo(count));
        for (int i = 0; i < count; i++)
        {
            var expected = fixture.GetProperty("nodes")[i];
            var message = result.Messages[i];
            Assert.Multiple(() =>
            {
                Assert.That(message.Type, Is.EqualTo(MessageType.Private));
                Assert.That(message.Contact.Uin, Is.EqualTo(expected.GetProperty("sender").GetInt64()));
                Assert.That(message.Contact.Nickname, Is.EqualTo(expected.GetProperty("name").GetString()));
                Assert.That(message.Contact.Uid, Is.EqualTo($"u_sender_{i}"));
                Assert.That(message.Receiver.Uin, Is.Zero);
                Assert.That(message.Time, Is.EqualTo(expected.GetProperty("time").GetInt64()));
                Assert.That(message.Sequence, Is.EqualTo(expected.GetProperty("sequence").GetUInt64()));
                Assert.That(message.ClientSequence, Is.EqualTo(expected.GetProperty("clientSequence").GetUInt64()));
                Assert.That(message.MessageId, Is.EqualTo(expected.GetProperty("msgUid").GetUInt64()));
                Assert.That(message.Random, Is.EqualTo(expected.GetProperty("random").GetUInt64()));
                Assert.That(((TextEntity)message.Entities.Single()).Text, Is.EqualTo(expected.GetProperty("text").GetString()));
                Assert.That(bot.CacheContext.ResolveCachedUid(message.Contact.Uin), Is.Null);
            });
        }
    }

    [TestCase(4294967296UL)]
    [TestCase(1791797400268224219UL)]
    [TestCase(ulong.MaxValue)]
    public async Task MessageAndFakeForwardPreserveAllRandomBits(ulong random)
    {
        using var bot = new BotContext(new(), new(), new());
        var packet = new CommonMessage
        {
            RoutingHead = new RoutingHead { FromUin = 10, FromUid = "sender", CommonC2C = new() { Name = "sender" } },
            ContentHead = new ContentHead { Type = 9, Sequence = 11, Time = 12, MsgUid = 13, ClientSequence = 14, Random = random }
        };
        var packer = new MessagePacker(bot);
        var message = await packer.ParseForwarded(packet);
        var fake = await packer.BuildFake(message);
        var decoded = ProtoHelper.Deserialize<CommonMessage>(ProtoHelper.Serialize(fake).Span);
        Assert.Multiple(() =>
        {
            Assert.That(message.Random, Is.EqualTo(random));
            Assert.That(decoded.ContentHead.Random, Is.EqualTo(random));
            Assert.That(decoded.ContentHead.Sequence, Is.EqualTo(11));
            Assert.That(decoded.ContentHead.Time, Is.EqualTo(12));
            Assert.That(decoded.ContentHead.MsgUid, Is.EqualTo(13));
            Assert.That(decoded.ContentHead.ClientSequence, Is.EqualTo(14));
        });
    }

    [Test]
    public void LiveTypeNineRemainsUnsupported()
    {
        using var bot = new BotContext(new(), new(), new());
        var packet = new CommonMessage { ContentHead = new() { Type = 9 }, RoutingHead = new() };
        var error = Assert.ThrowsAsync<OperationException>(() => new MessagePacker(bot).Parse(packet));
        Assert.That(error!.Message, Does.Contain("Unsupported incoming message type: 9"));
    }

    [TestCase(100, null, "100")]
    [TestCase(100, "", "100")]
    [TestCase(0, null, "")]
    [TestCase(100, "historical name", "historical name")]
    public async Task ForwardedIdentityUsesSnapshotAndDoesNotAssumeReceiver(long uin, string? name, string expected)
    {
        using var bot = new BotContext(new(), new BotKeystore { Uin = 200 }, new());
        var node = MessageProtocolReplay.ForwardNode();
        node.RoutingHead.FromUin = uin;
        node.RoutingHead.CommonC2C = name is null ? null! : new CommonC2C { Name = name };
        var message = await new MessagePacker(bot).ParseForwarded(node);
        Assert.Multiple(() =>
        {
            Assert.That(message.Contact.Nickname, Is.EqualTo(expected));
            Assert.That(message.Contact.Uin, Is.EqualTo(uin));
            Assert.That(message.Contact.Uid, Is.EqualTo("u_sender"));
            Assert.That(message.Receiver.Uin, Is.Zero);
            Assert.That(message.Receiver.Uid, Is.Empty);
        });
        node.RoutingHead.ToUin = 300;
        node.RoutingHead.ToUid = "u_recipient";
        var addressed = await new MessagePacker(bot).ParseForwarded(node);
        Assert.That(addressed.Receiver.Uin, Is.EqualTo(300));
        Assert.That(addressed.Receiver.Uid, Is.EqualTo("u_recipient"));
    }

    [Test]
    public async Task GroupForwardUsesHistoricalNameAndCardWithoutChangingCachedMember()
    {
        using var replay = new MessageProtocolReplay(_ => throw new AssertionException("Profiles must not be fetched."));
        var group = new BotGroup(400, "current group", 0, 0, 0, null, null, null);
        var current = new BotGroupMember(group, 100, "u_sender", "current name", GroupMemberPermission.Admin, 0, "current card", null, 0, 0, 0);
        replay.SeedContacts(group, current);
        var node = MessageProtocolReplay.ForwardNode();
        node.RoutingHead.Group = new CommonGroup { GroupCode = 400, GroupName = "historical group", GroupCard = "historical card" };
        var snapshot = await new MessagePacker(replay.Bot).ParseForwarded(node);
        Assert.Multiple(() =>
        {
            Assert.That(snapshot.Type, Is.EqualTo(MessageType.Group));
            Assert.That(snapshot.Contact.Nickname, Is.EqualTo("historical card"));
            Assert.That(((BotGroupMember)snapshot.Contact).MemberCard, Is.EqualTo("historical card"));
            Assert.That(snapshot.Receiver.Nickname, Is.EqualTo("historical group"));
            Assert.That(snapshot.Receiver, Is.SameAs(((BotGroupMember)snapshot.Contact).Group));
            Assert.That(replay.Requests, Is.Empty);
        });
        Assert.That((await replay.Bot.CacheContext.ResolveMember(400, 100))!.Value.Item2, Is.SameAs(current));
    }

    [Test]
    public async Task MixedForwardKeepsExistingMessageTypeRulesAndOrder()
    {
        using var replay = new MessageProtocolReplay(_ => throw new AssertionException("Unexpected profile lookup."));
        var group = new BotGroup(400, "group", 0, 0, 0, null, null, null);
        var member = new BotGroupMember(group, 100, "u_sender", "current", GroupMemberPermission.Member, 0, null, null, 0, 0, 0);
        replay.SeedContacts(group, member,
            new BotFriend(100, "current friend", "u_sender", "", "", "", null!),
            new BotFriend(200, "self", "u_self", "", "", "", null!));
        var nodes = new[] { 9, 82, 166, 141, 529 }.Select((type, index) =>
        {
            var node = MessageProtocolReplay.ForwardNode();
            node.ContentHead.Type = type;
            node.ContentHead.Sequence = (ulong)index;
            node.RoutingHead.ToUin = type == 82 ? 100 : 200;
            node.RoutingHead.ToUid = "u_self";
            if (type == 82) node.RoutingHead.Group = new CommonGroup { GroupCode = 400 };
            return node;
        }).ToArray();
        var result = (LongMsgRecvEventResp)await ((IService)new LongMsgRecvService()).Parse(MessageProtocolReplay.LongResponse(nodes), replay.Bot);
        Assert.That(result.Messages.Select(x => x.Type), Is.EqualTo(new[] { MessageType.Private, MessageType.Group, MessageType.Private, MessageType.Temp, MessageType.Private }));
        Assert.That(result.Messages.Select(x => x.Sequence), Is.EqualTo(new ulong[] { 0, 1, 2, 3, 4 }));
        Assert.That(result.Messages[0].Contact.Nickname, Is.EqualTo("snapshot"));
        Assert.That(result.Messages[1].Contact, Is.SameAs(member));
        Assert.That(result.Messages[2].Contact.Nickname, Is.EqualTo("current friend"));
        Assert.That(replay.Requests, Is.Empty);
    }

    [Test]
    public async Task NestedForwardDownloadsAndParsesTypeNineThroughNormalServices()
    {
        using var fixtures = MilkyLogFixture.Read();
        var payload = MilkyLogFixture.Bytes(fixtures.RootElement.GetProperty("forwards")[0]);
        using var replay = new MessageProtocolReplay(request =>
        {
            Assert.That(request.Command, Is.EqualTo(MessageProtocolReplay.LongMessageCommand));
            Assert.That(ProtoHelper.Deserialize<LongMsgInterfaceReq>(request.Data.Span).RecvReq.ResId, Is.EqualTo("nested"));
            return payload;
        });
        var result = (LongMsgRecvEventResp)await ((IService)new LongMsgRecvService())
            .Parse(MessageProtocolReplay.LongResponse(MessageProtocolReplay.ForwardNode("nested")), replay.Bot)
            .AsTask().WaitAsync(TimeSpan.FromSeconds(5));
        var nested = (MultiMsgEntity)result.Messages.Single().Entities.Single();
        Assert.That(nested.Messages, Has.Count.EqualTo(25));
        Assert.That(nested.Messages[24].Contact.Nickname, Is.EqualTo("sender-24"));
        Assert.That(((TextEntity)nested.Messages[24].Entities.Single()).Text, Is.EqualTo("message-24"));
        Assert.That(replay.Requests, Has.Count.EqualTo(1));
    }

    [TestCase(0x1234567887654321UL)]
    [TestCase(ulong.MaxValue)]
    public async Task LegacyProtocolBoundariesUseOnlyLowThirtyTwoBits(ulong random)
    {
        uint low = unchecked((uint)random);
        using var replay = new MessageProtocolReplay(request =>
        {
            if (request.Command == "trpc.msg.msg_svc.MsgService.SsoC2CRecallMsg")
            {
                var recall = ProtoHelper.Deserialize<SsoC2CRecallMsgReq>(request.Data.Span);
                Assert.That(recall.Info.Random, Is.EqualTo(low));
                Assert.That(recall.Info.MessageId, Is.EqualTo((0x01000000UL << 32) | low));
                return ReadOnlyMemory<byte>.Empty;
            }
            Assert.That(request.Command, Is.AnyOf("OidbSvcTrpcTcp.0xeac_1", "OidbSvcTrpcTcp.0xeac_2"));
            var oidb = ProtoHelper.Deserialize<Oidb>(request.Data.Span);
            Assert.That(ProtoHelper.Deserialize<EACReqBody>(oidb.Body.Span).Random, Is.EqualTo(low));
            return ProtoHelper.Serialize(new Oidb { Body = ProtoHelper.Serialize(new EACRspBody()) });
        });
        var sender = new BotFriend(100, "sender", "u_sender", "", "", "", null!);
        var receiver = new BotFriend(200, "self", "u_self", "", "", "", null!);
        var message = new BotMessage([new TextEntity("send")], sender, receiver, 123) { Random = random, Sequence = 77, ClientSequence = 78 };
        Assert.That(ProtoHelper.Deserialize<PbSendMsgReq>(MessagePacker.Build(message).Span).Random, Is.EqualTo(low));
        await replay.Bot.RecallMessage(message).WaitAsync(TimeSpan.FromSeconds(5));
        var group = new BotGroup(400, "group", 0, 0, 0, null, null, null);
        var member = new BotGroupMember(group, 100, "u_sender", "sender", GroupMemberPermission.Member, 0, null, null, 0, 0, 0);
        var grouped = new BotMessage(member, group, 123) { Random = random, Sequence = 77 };
        await replay.Bot.SetEssenceMessage(grouped).WaitAsync(TimeSpan.FromSeconds(5));
        await replay.Bot.RemoveEssenceMessage(grouped).WaitAsync(TimeSpan.FromSeconds(5));
        Assert.That(replay.Requests, Has.Count.EqualTo(3));
    }
}
