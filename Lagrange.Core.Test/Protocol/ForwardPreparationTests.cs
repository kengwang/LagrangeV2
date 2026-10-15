using Lagrange.Core.Common.Entity;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entities;
using Lagrange.Core.Utility;
using Lagrange.Testing;
namespace Lagrange.Core.Test.Protocol;
public sealed class ForwardPreparationTests
{
    private const string Pixel = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+aHZkAAAAASUVORK5CYII=";
    private static BotGroup Group() => new(400, "group", 0, 0, 0, null, null, null);
    private static BotFriend Friend() => new(500, "peer", "u_peer", "", "", "", null!);
    private static BotMessage Node(params IMessageEntity[] entities) => BotMessage.CreateCustomFriend(100, "historical", 0, "", 123, [..entities]);
    private static ReadOnlyMemory<byte> MediaResponse(Lagrange.Core.Common.Entity.BotSsoPacket packet, bool group)
    {
        var oidb = ProtoHelper.Deserialize<Oidb>(packet.Data.Span);
        var req = ProtoHelper.Deserialize<NTV2RichMediaReq>(oidb.Body.Span);
        Assert.That(req.ReqHead.Scene.SceneType, Is.EqualTo(group ? 2 : 1));
        if (group) Assert.That(req.ReqHead.Scene.Group!.GroupUin, Is.EqualTo(400));
        else Assert.That(req.ReqHead.Scene.C2C!.TargetUid, Is.EqualTo("u_peer"));
        var info = new MsgInfo { MsgInfoBody = req.Upload.UploadInfo.Select(x => new MsgInfoBody
        { Index = new IndexNode { Info = x.FileInfo, FileUuid = "fixture" } }).ToList(), ExtBizInfo = req.Upload.ExtBizInfo };
        return ProtoHelper.Serialize(new Oidb { Body = ProtoHelper.Serialize(new NTV2RichMediaResp
        { Upload = new UploadResp { MsgInfo = info, SubFileInfos = [new()] } }) });
    }
    [TestCase("image", true, 20u)]
    [TestCase("image", false, 10u)]
    [TestCase("record", true, 22u)]
    [TestCase("record", false, 12u)]
    [TestCase("video", true, 21u)]
    [TestCase("video", false, 11u)]
    public async Task FreshMediaIsPreparedForRealRecipient(string kind, bool group, uint business)
    {
        using var replay = new MessageProtocolReplay(p => MediaResponse(p, group));
        using var stream = new MemoryStream(Convert.FromBase64String(Pixel));
        RichMediaEntityBase media = kind switch { "image" => new ImageEntity(stream), "record" => new RecordEntity(stream), _ => new VideoEntity(stream) };
        var node = Node(media); node.Style = new() { BubbleId = 2160922 };
        var built = await new ForwardedMessageBuilder(replay.Bot, group ? Group() : Friend()).Build([node]);
        var packed = built.Items[0].Buffer.Msg.Single();
        Assert.That(packed.RoutingHead.FromUin, Is.EqualTo(100));
        Assert.That(packed.RoutingHead.CommonC2C.Name, Is.EqualTo("historical"));
        Assert.That(packed.ContentHead.Time, Is.EqualTo(123));
        Assert.That(packed.MessageBody!.RichText.Elems.Single(x => x.CommonElem is not null).CommonElem!.BusinessType, Is.EqualTo(business));
        Assert.That(packed.MessageBody.RichText.Elems.Any(x => x.ElemFlags2?.ColorTextId == 2160922), Is.True);
        Assert.That(replay.Requests, Has.Count.EqualTo(1));
        Assert.That(stream.CanRead, Is.True);
    }
    [Test]
    public async Task MentionsResolveOrBecomeTextWithoutChangingHistoricalIdentity()
    {
        using var replay = new MessageProtocolReplay(_ => throw new AssertionException("No network expected"));
        var group = Group();
        replay.SeedContacts(group, new BotGroupMember(group, 300, "u_member", "current", GroupMemberPermission.Member, 0, null, null, 0, 0, 0));
        var built = await new ForwardedMessageBuilder(replay.Bot, group).Build([Node(new MentionEntity(300, "old name"), new MentionEntity(301, "departed"), new MentionEntity(302, null), new MentionEntity(0, null))]);
        var elems = built.Items[0].Buffer.Msg.Single().MessageBody!.RichText.Elems;
        Assert.That(elems.Select(x => x.Text!.TextMsg), Is.EqualTo(new[] { "@old name", "@departed", "@302", "@全体成员" }));
        Assert.That(ProtoHelper.Deserialize<TextResvAttr>(elems[0].Text!.PbReserve.Span).AtMemberUid, Is.EqualTo("u_member"));
        Assert.That(elems[1].Text!.PbReserve.IsEmpty, Is.True);
    }

    [TestCase("image", true)]
    [TestCase("record", false)]
    [TestCase("video", true)]
    public async Task ParsedMediaKeepsItsOriginalBusinessTypeWithoutAnUploadStream(string kind, bool group)
    {
        using var replay = new MessageProtocolReplay(p => MediaResponse(p, group));
        using var stream = new MemoryStream(Convert.FromBase64String(Pixel));
        RichMediaEntityBase media = kind switch { "image" => new ImageEntity(stream), "record" => new RecordEntity(stream), _ => new VideoEntity(stream) };
        var first = await new ForwardedMessageBuilder(replay.Bot, group ? Group() : Friend()).Build([Node(media)]);
        var elems = first.Items[0].Buffer.Msg.Single().MessageBody!.RichText.Elems;
        var common = elems.Single(x => x.CommonElem is not null);
        IMessageEntity factory = kind switch { "image" => new ImageEntity(), "record" => new RecordEntity(), _ => new VideoEntity() };
        var parsed = factory.Parse(elems, common)!;
        var second = await new ForwardedMessageBuilder(replay.Bot, group ? Friend() : Group()).Build([Node(parsed)]);
        Assert.That(second.Items[0].Buffer.Msg.Single().MessageBody!.RichText.Elems.Single(x => x.CommonElem is not null).CommonElem!.BusinessType,
            Is.EqualTo(common.CommonElem!.BusinessType));
        Assert.That(replay.Requests, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task SharedMediaAndThreeLevelForwardsArePreparedOnlyOnce()
    {
        int uploads = 0;
        using var replay = new MessageProtocolReplay(p =>
        {
            if (p.Command.StartsWith("Oidb")) return MediaResponse(p, true);
            uploads++;
            return ProtoHelper.Serialize(new LongMsgInterfaceRsp { SendRsp = new() { ResId = $"child-{uploads}" } });
        });
        var stream = new TrackingStream(Convert.FromBase64String(Pixel));
        var media = new ImageEntity(stream, disposeOnCompletion: true);
        var inner = new MultiMsgEntity([Node(media)]);
        var middle = new MultiMsgEntity([Node(inner, media)]);
        var result = await new ForwardedMessageBuilder(replay.Bot, Group()).Build([Node(middle, inner, media)]);
        Assert.That(result.Items, Has.Count.EqualTo(3));
        Assert.That(uploads, Is.EqualTo(2));
        Assert.That(replay.Requests.Count(x => x.Command.StartsWith("Oidb")), Is.EqualTo(1));
        Assert.That(stream.DisposeCount, Is.EqualTo(1));
        NestedForwardTest.AssertReferences(result);
    }

    [Test]
    public void CycleReleasesAllOwnedStreamsBeforeAnyUpload()
    {
        using var replay = new MessageProtocolReplay(_ => throw new AssertionException("No upload before graph validation"));
        var stream = new TrackingStream([1]);
        var thumbnail = new TrackingStream([2]);
        var cyclic = new MultiMsgEntity();
        cyclic.Messages.Add(Node(cyclic));
        Assert.ThrowsAsync<OperationException>(() => new ForwardedMessageBuilder(replay.Bot, Group()).Build(
            [Node(new VideoEntity(stream, thumbnail, disposeOnCompletion: true), cyclic)]));
        Assert.That(stream.DisposeCount, Is.EqualTo(1));
        Assert.That(thumbnail.DisposeCount, Is.EqualTo(1));
        Assert.That(replay.Requests, Is.Empty);
    }

    [Test]
    public async Task FailedPreparationCleansLaterMediaAndNeverUploadsAForwardCard()
    {
        using var replay = new MessageProtocolReplay(_ => ProtoHelper.Serialize(new Oidb { Result = 7, Message = "upload denied" }));
        var first = new TrackingStream(Convert.FromBase64String(Pixel));
        var later = new TrackingStream([1]);
        var thumbnail = new TrackingStream([2]);
        var media = new ImageEntity(first, disposeOnCompletion: true);
        var failure = Assert.ThrowsAsync<ForwardPreparationException>(() => new ForwardedMessageBuilder(replay.Bot, Group()).Build(
            [Node(new MultiMsgEntity([Node(media)])), Node(new VideoEntity(later, thumbnail, disposeOnCompletion: true))]));
        Assert.That(failure!.NodePath, Does.Contain("messages[0].segments[0]"));
        Assert.That(failure.InnerException, Is.Not.Null);
        Assert.That(first.DisposeCount, Is.EqualTo(1));
        Assert.That(later.DisposeCount, Is.EqualTo(1));
        Assert.That(thumbnail.DisposeCount, Is.EqualTo(1));
        Assert.That(replay.Requests.All(x => x.Command.StartsWith("Oidb")), Is.True);
        var before = replay.Requests.Count;
        Assert.ThrowsAsync<ForwardPreparationException>(() => new ForwardedMessageBuilder(replay.Bot, Group()).Build([Node(media)]));
        Assert.That(replay.Requests.Count, Is.EqualTo(before));
        await Task.CompletedTask;
    }

    [Test]
    public void PartialUploadCannotBePackedOrRetriedEvenWhenMsgInfoWasAssigned()
    {
        using var replay = new MessageProtocolReplay(p => MediaResponse(p, true));
        var stream = new TrackingStream([1]);
        var media = new PartialUploadMedia(stream);
        Assert.ThrowsAsync<ForwardPreparationException>(() => new ForwardedMessageBuilder(replay.Bot, Group()).Build([Node(media)]));
        Assert.That(media.MsgInfo, Is.Not.Null);
        Assert.Throws<InvalidOperationException>(() => ((IMessageEntity)media).Build());
        Assert.ThrowsAsync<ForwardPreparationException>(() => new ForwardedMessageBuilder(replay.Bot, Group()).Build([Node(media)]));
        Assert.That(media.Attempts, Is.EqualTo(1));
        Assert.That(stream.DisposeCount, Is.EqualTo(1));
    }


    [TestCase(true)]
    [TestCase(false)]
    public void UploadUsesDestinationWhileDownloadUsesOriginalSource(bool group)
    {
        var oldGroup = new BotGroup(900, "source", 0, 0, 0, null, null, null);
        BotContact source = group ? new BotGroupMember(oldGroup, 100, "u_original", "old", GroupMemberPermission.Member, 0, null, null, 0, 0, 0)
            : new BotFriend(100, "old", "u_original", "", "", "", null!);
        var message = new BotMessage([], source, group ? Group() : Friend(), 123);
        var upload = NTV2RichMedia.BuildHead(message, new ImageEntity(), 100).Scene;
        var download = NTV2RichMedia.BuildHead(message, new ImageEntity(), 200).Scene;
        if (group)
        {
            Assert.That(upload.Group!.GroupUin, Is.EqualTo(400));
            Assert.That(download.Group!.GroupUin, Is.EqualTo(900));
        }
        else
        {
            Assert.That(upload.C2C!.TargetUid, Is.EqualTo("u_peer"));
            Assert.That(download.C2C!.TargetUid, Is.EqualTo("u_original"));
        }
    }

    [Test]
    public async Task TemporaryForwardMediaCarriesTheRealRecipientAndSourceGroup()
    {
        using var replay = new MessageProtocolReplay(p =>
        {
            var oidb = ProtoHelper.Deserialize<Oidb>(p.Data.Span);
            var req = ProtoHelper.Deserialize<NTV2RichMediaReq>(oidb.Body.Span);
            var route = ProtoHelper.Deserialize<SendRoutingHead>(req.ReqHead.Scene.C2C!.RoutingHead!.Value.Span);
            Assert.That(route.GroupTemp!.GroupUin, Is.EqualTo(700));
            Assert.That(route.GroupTemp.ToUid, Is.EqualTo("u_peer"));
            return MediaResponse(p, false);
        });
        var peer = new BotStranger(500, "peer", "u_peer", "", "", 0, default, 0, null, 0, "", "", "", null);
        using var stream = new MemoryStream(Convert.FromBase64String(Pixel));
        await new ForwardedMessageBuilder(replay.Bot, peer, 700).Build([Node(new ImageEntity(stream))]);
    }

    [Test]
    public void MentionLookupNetworkFailureIsNotSilentlyDowngraded()
    {
        using var replay = new MessageProtocolReplay(_ => throw new IOException("network down"));
        replay.SeedContacts(Friend());
        var error = Assert.ThrowsAsync<ForwardPreparationException>(() => new ForwardedMessageBuilder(replay.Bot, Friend()).Build([Node(new MentionEntity(300, "historical"))]));
        Assert.That(error!.SegmentType, Is.EqualTo(nameof(MentionEntity)));
        Assert.That(replay.Requests, Has.Count.EqualTo(1));
    }

    private sealed class TrackingStream(byte[] bytes) : MemoryStream(bytes)
    {
        public int DisposeCount { get; private set; }
        protected override void Dispose(bool disposing) { DisposeCount++; base.Dispose(disposing); }
    }

    private sealed class PartialUploadMedia : RichMediaEntityBase
    {
        internal override Lazy<Stream>? Stream { get; }
        public int Attempts { get; private set; }
        public PartialUploadMedia(Stream stream) { Own(stream); Stream = new(() => stream); }
        public override Task Preprocess(Lagrange.Core.BotContext context, BotMessage message)
        {
            if (!BeginPreparation(message)) return Task.CompletedTask;
            Attempts++;
            MsgInfo = new MsgInfo { MsgInfoBody = [new() { Index = new() { Info = new(), FileUuid = "partial" } }] };
            throw new IOException("File transfer failed after metadata response");
        }
        public override Task Postprocess(Lagrange.Core.BotContext context, BotMessage message) => Task.CompletedTask;
        internal override Elem[] Build() => [];
        internal override IMessageEntity? Parse(List<Elem> elements, Elem target) => null;
        public override string ToPreviewString() => "partial";
    }
}
