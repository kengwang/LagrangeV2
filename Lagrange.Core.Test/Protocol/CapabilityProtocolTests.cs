using System.Text.Json;
using System.Text.Json.Nodes;
using Lagrange.Codec;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Internal.Context;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Http;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Internal.Services.Http;
using Lagrange.Core.Internal.Services.Message;
using Lagrange.Core.Internal.Services.System;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entities;
using Lagrange.Core.Services;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Test.Protocol;

public sealed class CapabilityProtocolTests
{
    [Test] public async Task StickerUploadRetainsSubtypeAndPresentation()
    {
        using var stream = new MemoryStream(Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+jlbkAAAAASUVORK5CYII="));
        var image = new ImageEntity(stream, subType: 1);
        var friend = new BotFriend(123, "", "u_peer", "", "", "", null!);
        var message = new BotMessage([image], friend, friend, 1);
        var bytes = await ((IService)new ImageUploadService()).Build(new Lagrange.Core.Internal.Events.Message.ImageUploadEventReq(message, image), null!);
        var request = ProtoHelper.Deserialize<NTV2RichMediaReq>(ProtoHelper.Deserialize<Oidb>(bytes.Span).Body.Span);
        var picture = request.Upload.ExtBizInfo.Pic;
        Assert.That(picture.BizType, Is.EqualTo(1));
        Assert.That(picture.TextSummary, Is.EqualTo("[动画表情]"));
        Assert.That(ProtoHelper.Deserialize<LegacyImageReserve>(picture.BytesPbReserveC2c!).SubType, Is.EqualTo(1));
    }

    [Test] public void PrivateConfirmationSeparatesRequestAndServerSequences()
    {
        var friend = new BotFriend(123, "", "u_peer", "", "", "", null!);
        var message = new BotMessage(friend, friend, 1) { ClientSequence = 17 };
        message.ConfirmPrivateSequence(99);
        Assert.That(message.Sequence, Is.EqualTo(17));
        Assert.That(message.ClientSequence, Is.EqualTo(99));
    }

    [Test] public void QzoneReplyUsesTheConfirmedDataId()
    {
        var response = QzoneJsLiteralParser.ParseJsonResponse("callback({code:0,data:{id:12345}});");
        var reply = QzoneWebHttpService.ReadReply(response, "parent", 123);
        Assert.That(reply["comment_id"]!.GetValue<string>(), Is.EqualTo("12345"));
        Assert.That(reply["parent_comment_id"]!.GetValue<string>(), Is.EqualTo("parent"));
        Assert.Throws<InvalidDataException>(() => QzoneWebHttpService.ReadReply(new JsonObject(), "parent", 123));
    }

    [Test] public void ForwardedXmlReadsOnlyRootIdentity()
    {
        var xml = "<?xml version='1.0'?><!-- resid='wrong' --><msg m_resid='outer&amp;id' m_fileName='nested'><item><title>title</title><summary>summary</summary></item></msg>";
        var bytes = System.Text.Encoding.UTF8.GetBytes(xml);
        var elem = new Elem { RichMsg = new RichMsg { ServiceId = 35, BytesTemplate1 = new byte[] { 0 }.Concat(bytes).ToArray() } };
        var forward = (MultiMsgEntity)((IMessageEntity)new MultiMsgEntity()).Parse([elem], elem)!;
        Assert.That(forward.ResId, Is.EqualTo("outer&id"));
        Assert.That(forward.FileName, Is.EqualTo("nested"));
        Assert.That(forward.Summary, Is.EqualTo("summary"));
    }

    [Test] public async Task SystemFaceCatalogKeepsEveryMagicGroup()
    {
        var body = new D9154Resp { Magic = new D9154Magic { Lists = [new D9154List { PackName = "first", Emojis = [new D9154Emoji { Sid = "1" }] }, new D9154List { PackName = "second", Emojis = [new D9154Emoji { Sid = "2" }] }] } };
        var bytes = ProtoHelper.Serialize(new Oidb { Body = ProtoHelper.Serialize(body) });
        var response = (GetSystemFacesEventResp)await ((IService)new GetSystemFacesService()).Parse(bytes, null!);
        Assert.That(response.Packs.Select(x => x.PackName), Is.EqualTo(new[] { "first", "second" }));
        Assert.That(response.Packs.SelectMany(x => x.Faces).Select(x => x.Sid), Is.EqualTo(new[] { "1", "2" }));
    }

    [Test] public void PrivateHistoryErrorsRemainErrors()
    {
        using var bot = new BotContext(new(), new(), new());
        Assert.ThrowsAsync<Lagrange.Core.Exceptions.OperationException>(async () =>
            await ((IService)new GetRoamMessageService()).Parse(Convert.FromHexString("0805120664656e696564"), bot));
    }

    [Test] public void CategoryZeroIsExplicit() => Assert.That(Convert.ToHexString(ProtoHelper.Serialize(new GroupTagsRequest { GroupClass = 0 }).Span), Is.EqualTo("0800"));

    [TestCase(null, null, null)] [TestCase(true, true, null)] [TestCase(null, false, 5u)] [TestCase(null, true, 3u)]
    public void PermissionUpdatesRejectIncompleteOrInvalidLimits(bool? recall, bool? kick, uint? limit) => Assert.Throws<ArgumentException>(() => GroupManagementExt.BuildPermissions(recall, kick, limit));

    [Test] public void PermissionUpdatesContainOnlySpecifiedItems()
    {
        var items = GroupManagementExt.BuildPermissions(false, null, null);
        Assert.That(items, Has.Count.EqualTo(1));
        Assert.That(Convert.ToHexString(ProtoHelper.Serialize(items[0]).Span), Is.EqualTo("080210021A00"));
    }

    [Test] public void PermissionQueriesKeepUnknownValues()
    {
        var response = new AdminPermissionsResult(123, [new AdminPermissionMember { MemberUin = 456 }]);
        var value = GroupManagementExt.ReadPermissions(response, 123, 456);
        Assert.That(value.KickMember, Is.Null);
        Assert.That(value.RecallMemberMessage, Is.Null);
        Assert.Throws<InvalidDataException>(() => GroupManagementExt.ReadPermissions(response, 123, 789));
    }

    [Test] public void MissingSendConfirmationFails() => Assert.Throws<InvalidDataException>(() => SendMessageService.ReadConfirmation(new PbSendMsgResp()));
    [Test] public void ReviewRejectionIsReturned()
    {
        var response = ProtoHelper.Deserialize<PbSendMsgResp>(Convert.FromHexString("3001"));
        Assert.That(SendMessageService.ReadConfirmation(response).Result, Is.EqualTo(1));
    }
    [Test] public void ServerRejectionKeepsItsReason()
    {
        var response = ProtoHelper.Deserialize<PbSendMsgResp>(Convert.FromHexString("3A0A10051A0664656E696564"));
        var result = SendMessageService.ReadConfirmation(response);
        Assert.That(result.Result, Is.EqualTo(5));
        Assert.That(result.ErrorMessage, Is.EqualTo("denied"));
    }
    [TestCase("6002")] [TestCase("08014802")]
    public void FileReceiptDoesNotBecomeANewFile(string hex) => Assert.That(MessagePacker.IsFileReceipt(ProtoHelper.Deserialize<NotOnlineFile>(Convert.FromHexString(hex))), Is.True);

    [Test] public void NumericFilteredFriendCaptureKeepsItsAccountAndTimestamp()
    {
        // 首条脱敏好友申请记录。
        var bytes = Convert.FromHexString("0880c2d72f12066e69636b3030180020fe012a056d73673030320851512067726f75703a1766696c746572656420667269656e6420726571756573744080c3bbc2064880cee4cd02500062006a30abc8a360ea075bcf8bbfa1e1e0e010beb1ac5228138eb82586704b41691e9efea640392a2a65e9dd50de82f154e0201b");
        var item = D69DoubtItem.Parse(bytes);
        Assert.That(item.Uid, Is.EqualTo("100000000"));
        Assert.That(item.Nick, Is.EqualTo("nick00"));
        Assert.That(item.RequestTime, Is.EqualTo(1750000000));
        Assert.That(item.GroupCode, Is.EqualTo(700000000));
    }

    [TestCase("0000021D", null, 2, 29)] [TestCase("07E8021D", 2024, 2, 29)] [TestCase("07E7021D", null, null, null)]
    [TestCase("07E8041F", null, null, null)] [TestCase("07E8", null, null, null)]
    public void BirthdaysUseCalendarValidation(string hex, int? year, int? month, int? day) => Assert.That(FetchStrangerService.ReadBirthday(Convert.FromHexString(hex)), Is.EqualTo((year, month, day)));

    [Test] public void SilentAndPeakWaveformsUseRealPcm()
    {
        Assert.That(VoiceWaveform.FromPcm(new byte[60]), Is.EqualTo(Enumerable.Repeat((byte)25, 30)));
        var pcm = new byte[60]; pcm[0] = 0xff; pcm[1] = 0x7f;
        var bars = VoiceWaveform.FromPcm(pcm);
        Assert.That(bars[0], Is.EqualTo(255));
        Assert.That(bars.Skip(1), Is.All.Zero);
        Assert.Throws<ArgumentException>(() => VoiceWaveform.FromPcm([1]));
    }

    [Test] public void HighwayKeepsAddressPathAndChecksBlockIdentity()
    {
        Assert.That(HighwayContext.NormalizeAddress("127.0.0.1:8080", 123), Is.EqualTo("http://127.0.0.1:8080/cgi-bin/httpconn?htcmd=0x6FF0087&uin=123"));
        Assert.That(HighwayContext.NormalizeAddress("http://127.0.0.1:8080/upload?token=123", 123), Is.EqualTo("http://127.0.0.1:8080/upload?token=123"));
        var response = new RespDataHighwayHead { MsgSegHead = new SegHead { Filesize = 123, DataOffset = 0, DataLength = 30 } };
        Assert.DoesNotThrow(() => HighwayContext.ValidateAcknowledgement(response, 123, 0, 30));
        Assert.Throws<InvalidDataException>(() => HighwayContext.ValidateAcknowledgement(response, 123, 30, 30));
    }

    [TestCase(0)] [TestCase(5)] public void InvalidVideoConcurrencyIsRejected(int value) => Assert.Throws<ArgumentOutOfRangeException>(() => new BotConfig { VideoUploadConcurrency = value });

    [Test] public void QzoneCursorIsBoundToAccountAndOperation()
    {
        var state = QzoneFeedsHttpService.Initial(123);
        var cursor = QzonePublicExt.EncodeCursor(state);
        Assert.That(QzonePublicExt.DecodeCursor(cursor, 123, "friends").ToJsonString(), Is.EqualTo(state.ToJsonString()));
        Assert.Throws<ArgumentException>(() => QzonePublicExt.DecodeCursor(cursor, 456, "friends"));
        Assert.Throws<ArgumentException>(() => QzonePublicExt.DecodeCursor(cursor, 123, "home"));
        Assert.Throws<InvalidDataException>(() => QzoneFeedsHttpService.NextState(new JsonObject { ["icServerTime"] = 7 }, state));
    }

    [Test] public void QzoneProfileClearingDoesNotChangeOmittedFields()
    {
        var fields = QzoneWebHttpService.ProfileFields(123, "", null);
        Assert.That(fields["mb"], Is.EqualTo("2048"));
        Assert.That(fields["spacename"], Is.Empty);
        Assert.That(fields.ContainsKey("desc"), Is.False);
    }

    [TestCase("{x: (()=>1)()}")] [TestCase("{get x(){return 1;}}")]
    public void QzoneLiteralsRejectExecutableNodes(string source) => Assert.Throws<FormatException>(() => QzoneJsLiteralParser.Parse(source));

    [Test] public void QzoneLargeIntegersStayExact()
    {
        var root = QzoneJsLiteralParser.ParseJsonResponse("callback({code:0,id:18446744073709551615});");
        Assert.That(root["code"]!.GetValue<int>(), Is.Zero);
        Assert.That(root["id"]!.GetValue<ulong>(), Is.EqualTo(ulong.MaxValue));
    }

    [Test] public void QzoneContentExcludesCommentsAndPreservesLineBreaks()
    {
        var feed = new BotQzoneFeed { Key = "key", AppId = 311, UserId = 123,
            Html = "<div class='f-item' data-key='key'><input name='feed_data' data-uin='123' data-fkey='key' data-tid='post'><div class='f-info'>正文<br>第二行</div><div class='mod-comments'><div class='f-info'>评论</div></div></div>" };
        QzoneFeedContent.Populate(feed);
        Assert.That(feed.Content, Is.EqualTo("正文\n第二行"));
        Assert.That(feed.MessageId, Is.EqualTo("post"));
    }

    [Test] public async Task DeviceTextUsesSelfAccountAndDeviceRoute()
    {
        using var bot = new BotContext(new(), new BotKeystore { Uin = 123, Uid = "u_self" }, new());
        var self = new BotFriend(123, "", "u_self", "", "", "", null!);
        var outgoing = new BotMessage([new TextEntity("hello")], self, BotDeviceContact.Devices[1].AsFriend(), 10);
        var wire = ProtoHelper.Deserialize<PbSendMsgReq>(MessagePacker.Build(outgoing).Span);
        Assert.That(wire.RoutingHead.Trans0X211.ToUin, Is.EqualTo(123));
        Assert.That(wire.RoutingHead.Trans0X211.CcCmd, Is.EqualTo(7));
        var body = ProtoHelper.Deserialize<DatalineBody>(wire.MessageBody.MsgContent!.Value.Span);
        Assert.That(body.Header!.DestinationTerminal, Is.EqualTo(2));
        body.Header.SourceTerminal = 2; body.Header.DestinationTerminal = 1;
        var incoming = new CommonMessage { RoutingHead = new RoutingHead(), ContentHead = new ContentHead { Type = 529, SubType = 7, ClientSequence = 77 }, MessageBody = new MessageBody { MsgContent = ProtoHelper.Serialize(body) } };
        var message = await new MessagePacker(bot).Parse(incoming);
        Assert.That(message.Contact.Uin, Is.EqualTo(0x7e000002));
        Assert.That(message.ClientSequence, Is.EqualTo(77));
        Assert.That(message.Entities.Single(), Is.TypeOf<TextEntity>());
    }
}
