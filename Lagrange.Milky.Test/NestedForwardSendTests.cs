using Lagrange.Core.Internal.Packets.Service;
using System.IO.Compression;
using System.Text.Json.Nodes;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Internal.Services.Message;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entities;
using Lagrange.Core.Services;
using Lagrange.Core.Utility;
using Lagrange.Milky.Api.Handlers.Message;
using Lagrange.Milky.Caching;
using Lagrange.Milky.Converters;
using Lagrange.Milky.Serialization;
using Lagrange.Milky.Models.Segments;
using Lagrange.Testing;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lagrange.Milky.Test;

public sealed class NestedForwardSendTests
{
    [TestCase(2, "text")]
    [TestCase(2, "markdown")]
    [TestCase(3, "text")]
    public async Task MilkyGroupSendIncludesEveryNestedItem(int depth, string kind)
    {
        var uploads = new Dictionary<string, byte[]>();
        using var replay = new MessageProtocolReplay(request =>
        {
            if (request.Command.EndsWith("SsoSendLongMsg"))
            {
                var packet = ProtoHelper.Deserialize<LongMsgInterfaceReq>(request.Data.Span);
                Assert.That(packet.SendReq.GroupUin, Is.EqualTo(400));
                var id = $"resource-{uploads.Count}";
                uploads[id] = packet.SendReq.Payload;
                return ProtoHelper.Serialize(new LongMsgInterfaceRsp { SendRsp = new() { ResId = id } });
            }
            Assert.That(request.Command, Is.EqualTo("MessageSvc.PbSendMsg"));
            var send = ProtoHelper.Deserialize<PbSendMsgReq>(request.Data.Span);
            Assert.That(send.RoutingHead.Group.GroupUin, Is.EqualTo(400));
            var card = ReadCard(send.MessageBody.RichText.Elems.Single());
            Assert.That(uploads, Does.ContainKey(card["meta"]!["detail"]!["resid"]!.ToString()));
            return ProtoHelper.Serialize(new PbSendMsgResp { Sequence = 99, SendTime = 123 });
        });
        var group = new BotGroup(400, "group", 0, 0, 0, null, null, null);
        var member = new BotGroupMember(group, 200, "u_self", "self", GroupMemberPermission.Member, 0, null, null, 0, 0, 0);
        replay.SeedContacts(group, member);
        JsonNode segment = new JsonObject { ["type"] = kind, ["data"] = new JsonObject { [kind == "text" ? "text" : "content"] = "nested leaf" } };
        for (int i = 0; i < depth; i++) segment = new JsonObject
        {
            ["type"] = "forward", ["data"] = new JsonObject
            {
                ["messages"] = new JsonArray(new JsonObject { ["user_id"] = 100, ["sender_name"] = "historical", ["segments"] = new JsonArray(segment) }),
                ["title"] = $"layer-{i}", ["preview"] = new JsonArray("preview"), ["summary"] = "summary", ["prompt"] = "prompt"
            }
        };
        var request = new SendGroupMessageHandler.Request(400, [])
        {
            GroupId = 400, Message = [Serializer.JsonDeserialize<OutgoingSegmentBase>(segment.ToJsonString())!]
        };
        var converter = new MilkyConverter(replay.Bot, new MessageCache(replay.Bot, NullLoggerFactory.Instance), null!);
        var endpoint = new SendGroupMessageHandler(replay.Bot, converter);
        var response = await endpoint.ExecuteAsync(request, default).WaitAsync(TimeSpan.FromSeconds(5));
        Assert.That(((SendGroupMessageHandler.Result)response.Data!).MessageSeq, Is.EqualTo(99));
        Assert.That(uploads, Has.Count.EqualTo(depth));
        var compressed = uploads[$"resource-{depth - 1}"];
        using var source = new MemoryStream(compressed);
        using var gzip = new GZipStream(source, CompressionMode.Decompress);
        using var dest = new MemoryStream();
        gzip.CopyTo(dest);
        var content = ProtoHelper.Deserialize<PbMultiMsgTransmit>(dest.ToArray());
        Assert.That(content.Items, Has.Count.EqualTo(depth));
        foreach (var card in content.Items.SelectMany(x => x.Buffer.Msg).SelectMany(x => x.MessageBody!.RichText.Elems).Where(x => x.LightAppElem is not null).Select(ReadCard))
        {
            var name = JsonNode.Parse(card["extra"]!.ToString())!["filename"]!.ToString();
            Assert.That(content.Items.Select(x => x.FileName), Does.Contain(name));
        }
        var parsed = (LongMsgRecvEventResp)await ((IService)new LongMsgRecvService()).Parse(
            ProtoHelper.Serialize(new LongMsgInterfaceRsp { RecvRsp = new() { Payload = compressed, ResId = "root" } }), replay.Bot);
        var leaf = parsed.Messages.Single();
        for (int i = 1; i < depth; i++) leaf = ((MultiMsgEntity)leaf.Entities.Single()).Messages.Single();
        Assert.That(leaf.Entities.Single(), kind == "text" ? Is.TypeOf<TextEntity>() : Is.TypeOf<MarkdownEntity>());
        Assert.That(replay.Requests, Has.Count.EqualTo(depth + 1));
    }


    [TestCase(true, false)]
    [TestCase(false, false)]
    [TestCase(true, true)]
    public async Task MediaAndMentionsGoThroughTheRealSendPipeline(bool groupSend, bool rejectUpload)
    {
        PbMultiMsgTransmit? uploaded = null;
        using var replay = new MessageProtocolReplay(p =>
        {
            if (p.Command.StartsWith("Oidb"))
            {
                var oidb = ProtoHelper.Deserialize<Oidb>(p.Data.Span);
                if (rejectUpload) return ProtoHelper.Serialize(new Oidb { Result = 7, Message = "upload denied" });
                var req = ProtoHelper.Deserialize<NTV2RichMediaReq>(oidb.Body.Span);
                Assert.That(req.ReqHead.Scene.SceneType, Is.EqualTo(groupSend ? 2 : 1));
                if (groupSend) Assert.That(req.ReqHead.Scene.Group!.GroupUin, Is.EqualTo(400));
                else Assert.That(req.ReqHead.Scene.C2C!.TargetUid, Is.EqualTo("u_peer"));
                return ProtoHelper.Serialize(new Oidb { Body = ProtoHelper.Serialize(new NTV2RichMediaResp
                { Upload = new UploadResp { MsgInfo = new MsgInfo { MsgInfoBody = req.Upload.UploadInfo.Select(x => new MsgInfoBody
                  { Index = new IndexNode { Info = x.FileInfo, FileUuid = "image-ref" } }).ToList(), ExtBizInfo = req.Upload.ExtBizInfo } } }) });
            }
            if (p.Command.EndsWith("SsoSendLongMsg"))
            {
                var request = ProtoHelper.Deserialize<LongMsgInterfaceReq>(p.Data.Span);
                using var input = new MemoryStream(request.SendReq.Payload);
                using var gzip = new GZipStream(input, CompressionMode.Decompress);
                using var output = new MemoryStream(); gzip.CopyTo(output);
                uploaded = ProtoHelper.Deserialize<PbMultiMsgTransmit>(output.ToArray());
                return ProtoHelper.Serialize(new LongMsgInterfaceRsp { SendRsp = new() { ResId = "ready-forward" } });
            }
            Assert.That(p.Command, Is.EqualTo("MessageSvc.PbSendMsg"));
            return ProtoHelper.Serialize(new PbSendMsgResp { Sequence = 99, SendTime = 123 });
        });
        var group = new BotGroup(400, "group", 0, 0, 0, null, null, null);
        replay.SeedContacts(group, new BotGroupMember(group, 200, "u_self", "self", GroupMemberPermission.Member, 0, null, null, 0, 0, 0),
            new BotGroupMember(group, 300, "u_member", "current", GroupMemberPermission.Member, 0, null, null, 0, 0, 0),
            new BotFriend(300, "current", "u_member", "", "", "", null!), new BotFriend(200, "self", "u_self", "", "", "", null!), new BotFriend(500, "peer", "u_peer", "", "", "", null!));
        const string json = """{"type":"forward","data":{"messages":[{"user_id":100,"sender_name":"historical","message_style":{"bubble_id":2160922},"segments":[            {"type":"image","data":{"uri":"base64://iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+aHZkAAAAASUVORK5CYII="}},            {"type":"mention","data":{"user_id":300,"name":"historical mention"}},            {"type":"mention_all","data":{}}]}]}}""";
        var segment = Serializer.JsonDeserialize<OutgoingSegmentBase>(json)!;
        var converter = new MilkyConverter(replay.Bot, new MessageCache(replay.Bot, NullLoggerFactory.Instance), new ResourceConverter());
        Lagrange.Milky.Api.MilkyApiResponse response = groupSend
            ? await new SendGroupMessageHandler(replay.Bot, converter).ExecuteAsync(new(400, [segment]) { GroupId = 400, Message = [segment] }, default)
            : await new SendPrivateMessageHandler(replay.Bot, converter).ExecuteAsync(new(500, [segment]) { UserId = 500, Message = [segment] }, default);
        if (rejectUpload)
        {
            Assert.That(response.Status, Is.EqualTo("failed"));
            Assert.That(response.Message, Does.Contain("root[0].segments[0]"));
            Assert.That(response.Message, Does.Not.Contain("upload denied"));
            Assert.That(response.Data, Is.Null);
            Assert.That(uploaded, Is.Null);
            Assert.That(replay.Requests.All(x => x.Command.StartsWith("Oidb")), Is.True);
        }
        else
        {
            Assert.That(response.Status, Is.EqualTo("ok"));
            var packed = uploaded!.Items[0].Buffer.Msg.Single();
            Assert.That(packed.RoutingHead.FromUin, Is.EqualTo(100));
            Assert.That(packed.RoutingHead.CommonC2C.Name, Is.EqualTo("historical"));
            Assert.That(packed.MessageBody!.RichText.Elems.Any(x => x.Text?.TextMsg == "@historical mention"), Is.True);
            Assert.That(packed.MessageBody.RichText.Elems.Any(x => x.Text?.TextMsg == "@全体成员"), Is.True);
            Assert.That(packed.MessageBody.RichText.Elems.Single(x => x.CommonElem is not null).CommonElem!.BusinessType, Is.EqualTo(groupSend ? 20 : 10));
            Assert.That(packed.MessageBody.RichText.Elems.Any(x => x.ElemFlags2?.ColorTextId == 2160922), Is.True);
        }
    }

    [Test]
    public async Task ConversionFailureReleasesPreviouslyOpenedMedia()
    {
        using var replay = new MessageProtocolReplay(_ => throw new AssertionException("Conversion must not send packets"));
        var converter = new MilkyConverter(replay.Bot, new MessageCache(replay.Bot, NullLoggerFactory.Instance), new ResourceConverter());
        var path = Path.GetTempFileName();
        try
        {
            var message = new JsonObject { ["type"] = "forward", ["data"] = new JsonObject { ["messages"] = new JsonArray(new JsonObject
            {
                ["user_id"] = 100, ["sender_name"] = "name", ["segments"] = new JsonArray(
                    new JsonObject { ["type"] = "image", ["data"] = new JsonObject { ["uri"] = new Uri(path).AbsoluteUri } },
                    new JsonObject { ["type"] = "image", ["data"] = new JsonObject { ["uri"] = "invalid://image" } })
            }) } };
            var segment = Serializer.JsonDeserialize<OutgoingSegmentBase>(message.ToJsonString())!;
            Assert.ThrowsAsync<NotSupportedException>(() => converter.FromOutgoingSegmentsAsync([segment], MessageType.Group, 400));
            using var exclusive = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            Assert.That(replay.Requests, Is.Empty);
        }
        finally { File.Delete(path); }
        await Task.CompletedTask;
    }

    private static JsonNode ReadCard(Elem elem)
    {
        using var source = new MemoryStream(elem.LightAppElem!.BytesData.ToArray()[1..]);
        using var zlib = new ZLibStream(source, CompressionMode.Decompress);
        using var reader = new StreamReader(zlib);
        return JsonNode.Parse(reader.ReadToEnd())!;
    }
}
