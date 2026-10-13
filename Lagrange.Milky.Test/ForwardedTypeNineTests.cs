using Lagrange.Core.Common.Entity;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entities;
using Lagrange.Core.Utility;
using Lagrange.Milky.Api.Handlers.Message;
using Lagrange.Milky.Caching;
using Lagrange.Milky.Converters;
using Lagrange.Milky.Models.Messages;
using Lagrange.Milky.Models.Segments;
using Lagrange.Testing;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lagrange.Milky.Test;

public sealed class ForwardedTypeNineTests
{
    [TestCase(0, 25)]
    [TestCase(1, 6)]
    [TestCase(2, 25)]
    public async Task ForwardedMessagesEndpointConvertsEveryCapturedNode(int index, int count)
    {
        using var fixtures = MilkyLogFixture.Read();
        var fixture = fixtures.RootElement.GetProperty("forwards")[index];
        var payload = MilkyLogFixture.Bytes(fixture);
        using var replay = new MessageProtocolReplay(request =>
        {
            Assert.That(request.Command, Is.EqualTo(MessageProtocolReplay.LongMessageCommand));
            return payload;
        });
        var converter = new MilkyConverter(replay.Bot, new MessageCache(replay.Bot, NullLoggerFactory.Instance), null!);
        var endpoint = new GetForwardedMessagesHandler(replay.Bot, converter);
        var response = await endpoint.ExecuteAsync(new("test-forward") { ForwardId = "test-forward" }, default)
            .WaitAsync(TimeSpan.FromSeconds(5));
        var result = (GetForwardedMessagesHandler.Result)response.Data!;
        Assert.That(response.Status, Is.EqualTo("ok"));
        Assert.That(result.Messages, Has.Length.EqualTo(count));
        for (int i = 0; i < count; i++)
        {
            var expected = fixture.GetProperty("nodes")[i];
            var message = result.Messages[i];
            Assert.Multiple(() =>
            {
                Assert.That(message.SenderName, Is.EqualTo(expected.GetProperty("name").GetString()));
                Assert.That(message.MessageSeq, Is.EqualTo(expected.GetProperty("sequence").GetUInt64()));
                Assert.That(message.Time, Is.EqualTo(expected.GetProperty("time").GetInt64()));
                Assert.That(((TextIncomingSegment)message.Segments.Single()).Data.Text, Is.EqualTo(expected.GetProperty("text").GetString()));
            });
        }
        Assert.That(replay.Requests, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task ReplyHistoryLookupWithTypeNineForwardKeepsCurrentMessageAndOtherSegments()
    {
        using var fixtures = MilkyLogFixture.Read();
        var payload = MilkyLogFixture.Bytes(fixtures.RootElement.GetProperty("forwards")[1]);
        var referenced = MessageProtocolReplay.ForwardNode("nested-forward");
        referenced.ContentHead.Type = 82;
        referenced.RoutingHead.ToUin = 100;
        referenced.RoutingHead.Group = new CommonGroup { GroupCode = 400, GroupName = "group" };
        using var replay = new MessageProtocolReplay(request =>
        {
            if (request.Command == MessageProtocolReplay.LongMessageCommand) return payload;
            Assert.That(request.Command, Is.EqualTo("trpc.msg.register_proxy.RegisterProxy.SsoGetGroupMsg"));
            var lookup = ProtoHelper.Deserialize<SsoGetGroupMsg>(request.Data.Span);
            Assert.That(lookup.Info.GroupUin, Is.EqualTo(400));
            Assert.That(lookup.Info.StartSequence, Is.EqualTo(77));
            return ProtoHelper.Serialize(new SsoGetGroupMsgRsp
            {
                Body = new SsoGetGroupMsgRspBody { Messages = [referenced] }
            });
        });
        var group = new BotGroup(400, "group", 0, 0, 0, null, null, null);
        var member = new BotGroupMember(group, 100, "u_sender", "sender", GroupMemberPermission.Member, 0, null, null, 0, 0, 0);
        replay.SeedContacts(group, member);
        var source = new BotMessage(member, group, 123) { Sequence = 77 };
        var current = new BotMessage([new ReplyEntity(source), new TextEntity("still delivered")], member, group, 456) { Sequence = 99 };
        var converter = new MilkyConverter(replay.Bot, new MessageCache(replay.Bot, NullLoggerFactory.Instance), null!);
        var message = (GroupIncomingMessage)await converter.ToIncomingMessageAsync(current)
            .WaitAsync(TimeSpan.FromSeconds(5));
        var reply = ((ReplyIncomingSegment)message.Segments[0]).Data;
        var forward = ((ForwardIncomingSegment)reply.Segments.Single()).Data;
        Assert.Multiple(() =>
        {
            Assert.That(message.MessageSeq, Is.EqualTo(99));
            Assert.That(message.Segments, Has.Count.EqualTo(2));
            Assert.That(reply.MessageSeq, Is.EqualTo(77));
            Assert.That(reply.SenderId, Is.EqualTo(100));
            Assert.That(forward.ForwardId, Is.EqualTo("nested-forward"));
            Assert.That(forward.Title, Is.EqualTo("聊天记录"));
            Assert.That(((TextIncomingSegment)message.Segments[1]).Data.Text, Is.EqualTo("still delivered"));
            Assert.That(replay.Requests.Select(x => x.Command), Is.EqualTo(new[]
            {
                "trpc.msg.register_proxy.RegisterProxy.SsoGetGroupMsg", MessageProtocolReplay.LongMessageCommand
            }));
        });
    }
}
