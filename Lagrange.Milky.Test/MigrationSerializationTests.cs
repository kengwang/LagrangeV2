using System.Text.Json;
using Lagrange.Core.Message.Entities;
using Lagrange.Milky.Api.Handlers.Message;
using Lagrange.Milky.Models.Messages;
using Lagrange.Milky.Models.Segments;
using Lagrange.Milky.Serialization;
using Lagrange_Milky;
using Lagrange.Core.Message;
using Lagrange.Milky.Caching;
using Lagrange.Milky.Converters;
using Lagrange.Milky.Api.Handlers.System;
using Lagrange.Milky.Api.Handlers.Interaction;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lagrange.Milky.Test;

public class MigrationSerializationTests
{
    [Test]
    public void HistorySyncContractsUseGeneratedMetadata()
    {
        var options = new JsonSerializerOptions().AddSerializerContextsFromLagrange_Milky();
        var probe = (ProbeHistorySyncStateHandler.Request)JsonSerializer.Deserialize(
            """{"group_ids":[123],"private_targets":[{"user_id":456,"uid":"u"}]}""",
            options.GetTypeInfo(typeof(ProbeHistorySyncStateHandler.Request)))!;
        Assert.That(probe.GroupIds.Single(), Is.EqualTo(123));
        Assert.That(probe.PrivateTargets.Single().Uid, Is.EqualTo("u"));
        var result = new GetPrivateHistoryRoamPageHandler.Result { NextCursor = new(99, 7) };
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(result, options.GetTypeInfo(typeof(GetPrivateHistoryRoamPageHandler.Result))));
        Assert.That(json.RootElement.GetProperty("next_cursor").GetProperty("random").GetUInt32(), Is.EqualTo(7));
        Assert.That(json.RootElement.GetProperty("is_complete").GetBoolean(), Is.False);
    }
    [Test]
    public void MissingTemporaryReplyIsRejectedWithoutInventingReference()
    {
        var converter = new MilkyConverter(null!, new MessageCache(null!, NullLoggerFactory.Instance), null!);
        var error = Assert.ThrowsAsync<InvalidOperationException>(async () => await converter.FromOutgoingSegmentsAsync(
            [new ReplyOutgoingSegment { Data = new ReplyOutgoingSegmentData(123) { MessageSeq = 123 } }], MessageType.Temp, 456, CancellationToken.None));
        Assert.That(error!.Message, Does.Contain("123"));
    }

    [Test]
    public void ExtendedRequestsHaveGeneratedMetadataWithoutReflection()
    {
        var options = new JsonSerializerOptions().AddSerializerContextsFromLagrange_Milky();
        var key = (RequestDatabaseKeyHandler.Request)JsonSerializer.Deserialize(
            "{\"db_salt\":\"aabb\"}", options.GetTypeInfo(typeof(RequestDatabaseKeyHandler.Request)))!;
        Assert.That(key.DbSalt, Is.EqualTo("aabb"));
        var button = (ClickInlineKeyboardButtonHandler.Request)JsonSerializer.Deserialize(
            """{"group_id":1,"app_id":2,"message_seq":3,"button_id":"b","callback_data":"c"}""",
            options.GetTypeInfo(typeof(ClickInlineKeyboardButtonHandler.Request)))!;
        Assert.That(button.GroupId, Is.EqualTo(1));
        Assert.That(button.AppId, Is.EqualTo(2));
        Assert.That(button.MessageSeq, Is.EqualTo(3));
        Assert.That(button.ButtonId, Is.EqualTo("b"));
        Assert.That(button.CallbackData, Is.EqualTo("c"));
    }

    [Test]
    public void TemporaryMessageRetainsSourceGroupAndFlashFileWithoutReflection()
    {
        Assert.That(JsonSerializer.IsReflectionEnabledByDefault, Is.False);
        const string input = """
            {"message_scene":"temp","group_id":123,"peer_id":456,"sender_id":456,"message_seq":789,"time":42,
             "segments":[{"type":"flash_file","data":{"fileset_id":"fs-123","file_name":"report.zip","thumb_url":"https://example.test/thumb","scene_type":2}}]}
            """;

        var message = Serializer.JsonDeserialize<IncomingMessageBase>(input);
        Assert.That(message, Is.TypeOf<TempIncomingMessage>());
        var temporary = (TempIncomingMessage)message!;
        Assert.That(temporary.GroupId, Is.EqualTo(123));
        Assert.That(temporary.PeerId, Is.EqualTo(456));
        Assert.That(temporary.Segments.Single(), Is.TypeOf<FlashFileIncomingSegment>());
        var flash = (FlashFileIncomingSegment)temporary.Segments.Single();
        Assert.That(flash.Data.FilesetId, Is.EqualTo("fs-123"));
        Assert.That(flash.Data.FileName, Is.EqualTo("report.zip"));

        using var output = JsonDocument.Parse(Serializer.JsonSerialize<IncomingMessageBase>(temporary));
        Assert.That(output.RootElement.GetProperty("message_scene").GetString(), Is.EqualTo("temp"));
        Assert.That(output.RootElement.GetProperty("group_id").GetInt64(), Is.EqualTo(123));
        Assert.That(output.RootElement.GetProperty("segments")[0].GetProperty("type").GetString(), Is.EqualTo("flash_file"));
    }

    [Test]
    public void TemporarySendRequestUsesGeneratedEndpointMetadata()
    {
        const string input = """
            {"group_id":123,"user_id":456,"message":[{"type":"text","data":{"text":"hello"}}]}
            """;
        var options = new JsonSerializerOptions().AddSerializerContextsFromLagrange_Milky();
        var request = (SendTempMessageHandler.Request?)JsonSerializer.Deserialize(
            input, options.GetTypeInfo(typeof(SendTempMessageHandler.Request)));
        Assert.That(request, Is.Not.Null);
        Assert.That(request!.GroupId, Is.EqualTo(123));
        Assert.That(request.UserId, Is.EqualTo(456));
        Assert.That(request.Message.Single(), Is.TypeOf<TextOutgoingSegment>());
        Assert.That(((TextOutgoingSegment)request.Message.Single()).Data.Text, Is.EqualTo("hello"));
    }

    [Test]
    public void KeyboardRetainsNewActionFieldsWithoutReflection()
    {
        const string keyboardJson = """
            {"Rows":[{"Buttons":[{"Id":"button","Action":{"ClickLimit":3,"AtBotShowChannelList":true,"Anchor":7,"Data":"callback"}}]}],"BotAppId":99}
            """;
        var keyboard = new KeyboardEntity(keyboardJson);
        var action = keyboard.Data.Rows.Single().Buttons.Single().Action;
        Assert.That(action.ClickLimit, Is.EqualTo(3));
        Assert.That(action.AtBotShowChannelList, Is.True);
        Assert.That(action.Anchor, Is.EqualTo(7));

        IncomingSegmentBase segment = new KeyboardIncomingSegment
        {
            Data = new KeyboardIncomingSegmentData { Data = keyboard.ToJson() }
        };
        using var output = JsonDocument.Parse(Serializer.JsonSerialize(segment));
        using var payload = JsonDocument.Parse(output.RootElement.GetProperty("data").GetProperty("data").GetString()!);
        var outputAction = payload.RootElement.GetProperty("Rows")[0].GetProperty("Buttons")[0].GetProperty("Action");
        Assert.That(outputAction.GetProperty("ClickLimit").GetUInt32(), Is.EqualTo(3));
        Assert.That(outputAction.GetProperty("AtBotShowChannelList").GetBoolean(), Is.True);
        Assert.That(outputAction.GetProperty("Anchor").GetUInt32(), Is.EqualTo(7));
    }
}
