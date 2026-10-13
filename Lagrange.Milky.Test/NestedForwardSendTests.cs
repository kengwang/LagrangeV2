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

    private static JsonNode ReadCard(Elem elem)
    {
        using var source = new MemoryStream(elem.LightAppElem!.BytesData.ToArray()[1..]);
        using var zlib = new ZLibStream(source, CompressionMode.Decompress);
        using var reader = new StreamReader(zlib);
        return JsonNode.Parse(reader.ReadToEnd())!;
    }
}
