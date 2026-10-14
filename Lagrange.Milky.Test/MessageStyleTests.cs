using System.Text.Json;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entities;
using Lagrange.Milky.Api.Handlers.Message;
using Lagrange.Milky.Caching;
using Lagrange.Milky.Converters;
using Lagrange.Milky.Models.Messages;
using Lagrange.Milky.Models.Segments;
using Lagrange.Milky.Serialization;
using Lagrange.Testing;
using Microsoft.Extensions.Logging.Abstractions;
using Lagrange_Milky;

namespace Lagrange.Milky.Test;

public sealed class MessageStyleTests
{
    [TestCase("")]
    [TestCase(",\"message_style\":null")]
    [TestCase(",\"message_style\":{}")]
    [TestCase(",\"message_style\":{\"bubble_id\":18446744073709551615,\"pendant_id\":18446744073709551615,\"font_id\":65535,\"font_effect_id\":4294967295,\"bubble_diy_text_id\":4294967295,\"is_cs_font_effect_enabled\":true}")]
    public void SendRequestsPreserveDefaultsAndBoundaries(string style)
    {
        var json = "{\"group_id\":123,\"user_id\":100,\"message\":[]" + style + "}";
        var options = new JsonSerializerOptions().AddSerializerContextsFromLagrange_Milky();
        var group = JsonSerializer.Deserialize<SendGroupMessageHandler.Request>(json, options)!;
        var friend = JsonSerializer.Deserialize<SendPrivateMessageHandler.Request>(json, options)!;
        var temp = JsonSerializer.Deserialize<SendTempMessageHandler.Request>(json, options)!;
        foreach (var actual in new[] { group.MessageStyle, friend.MessageStyle, temp.MessageStyle })
        {
            if (style.Length == 0 || style.Contains("null")) Assert.That(actual, Is.Null);
            else if (style.EndsWith("{}")) Assert.That(actual!.ToCore().BubbleId, Is.Zero);
            else
            {
                Assert.That(actual!.BubbleId, Is.EqualTo(ulong.MaxValue));
                Assert.That(actual.PendantId, Is.EqualTo(ulong.MaxValue));
                Assert.That(actual.FontId, Is.EqualTo(ushort.MaxValue));
                Assert.That(actual.FontEffectId, Is.EqualTo(uint.MaxValue));
                Assert.That(actual.BubbleDiyTextId, Is.EqualTo(uint.MaxValue));
                Assert.That(actual.IsCsFontEffectEnabled, Is.True);
            }
        }
    }

    [Test]
    public async Task NestedForwardStylesAreIndependentAndReported()
    {
        var converter = new MilkyConverter(null!, new MessageCache(null!, NullLoggerFactory.Instance), null!);
        var input = Serializer.JsonDeserialize<OutgoingSegmentBase>("""
            {"type":"forward","data":{"messages":[{"user_id":100,"sender_name":"outer","message_style":{"bubble_id":7},"segments":[{"type":"forward","data":{"messages":[{"user_id":101,"sender_name":"inner","message_style":{"pendant_id":8},"segments":[{"type":"text","data":{"text":"styled"}}]},{"user_id":102,"sender_name":"plain","segments":[{"type":"text","data":{"text":"plain"}}]}]}}]}]}}
            """)!;
        var chain = await converter.FromOutgoingSegmentsAsync([input], MessageType.Group, 123);
        Assert.That(chain.Style, Is.Null);
        var outer = ((MultiMsgEntity)chain.Single()).Messages.Single();
        Assert.That(outer.Style!.BubbleId, Is.EqualTo(7));
        var inner = ((MultiMsgEntity)outer.Entities.Single()).Messages;
        Assert.That(inner[0].Style!.PendantId, Is.EqualTo(8));
        Assert.That(inner[0].Style!.BubbleId, Is.Zero);
        Assert.That(inner[1].Style, Is.Null);

        var longMessage = new LongMsgEntity();
        longMessage.Messages.AddRange(inner);
        var segments = await converter.ToIncomingSegmentsAsync([longMessage], MessageType.Group, 123);
        var nodes = ((LongMsgIncomingSegment)segments.Single()).Data.Messages!;
        using var json = JsonDocument.Parse(Serializer.JsonSerialize(nodes[0]));
        Assert.That(json.RootElement.GetProperty("message_style").GetProperty("pendant_id").GetUInt64(), Is.EqualTo(8));
        Assert.That(Serializer.JsonSerialize(nodes[1]), Does.Not.Contain("message_style"));
    }

    [TestCase(MessageType.Private)]
    [TestCase(MessageType.Group)]
    [TestCase(MessageType.Temp)]
    public async Task ReceivedAndHistoryMessagesUseSameStyleConversion(MessageType type)
    {
        using var replay = new MessageProtocolReplay(_ => throw new InvalidOperationException());
        var converter = new MilkyConverter(replay.Bot, new MessageCache(replay.Bot, NullLoggerFactory.Instance), null!);
        var message = new BotMessage(
            new MessageBuilder().Text("hello").Style(new() { BubbleId = 7, PendantId = 8,
                FontId = 0x1234, FontEffectId = 2000, IsCsFontEffectEnabled = true, BubbleDiyTextId = 9 }).Build(),
            new BotFriend(100, "sender", "u_sender", "", "", "", new(0, "default", 1, 0)),
            new BotFriend(200, "self", "u_self", "", "", "", new(0, "default", 1, 0)), 123);
        if (type == MessageType.Group)
            message = BotMessage.CreateCustomGroup(400, 100, "sender", 123, message.Entities);
        else if (type == MessageType.Temp)
            message.TempGroupUin = 400;
        var incoming = await converter.ToIncomingMessageAsync(message);
        using var json = JsonDocument.Parse(Serializer.JsonSerialize(incoming));
        var style = json.RootElement.GetProperty("message_style");
        Assert.That(style.EnumerateObject().Count(), Is.EqualTo(6));
        Assert.That(style.GetProperty("font_id").GetUInt16(), Is.EqualTo(0x1234));
        var history = GetHistoryMessagesHandler.CreateResult([incoming]);
        Assert.That(history.Messages.Single().MessageStyle!.PendantId, Is.EqualTo(8));
        message.Style = null;
        Assert.That(Serializer.JsonSerialize(await converter.ToIncomingMessageAsync(message)), Does.Not.Contain("message_style"));
    }
}
