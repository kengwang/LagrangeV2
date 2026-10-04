using System.Text;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Context;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Internal.Packets.Service.Migration;
using Lagrange.Core.Message.Entities;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Test.Protocol;

public sealed class MigrationCompletionTest
{
    [Test]
    public void VoiceTranscriptionValidatesIdentityBeforeTransport()
    {
        var voice = new VoiceContext(null!);
        Assert.ThrowsAsync<ArgumentNullException>(() => voice.Transcribe(null!, CancellationToken.None));
        var input = new BotVoiceTranscription(true, 0, 1, 2, "uuid", new string('a', 32), 1, 2, 1);
        Assert.ThrowsAsync<ArgumentException>(() => voice.Transcribe(input, CancellationToken.None));
        Assert.ThrowsAsync<ArgumentException>(() => voice.Transcribe(input with { MessageId = 1, Md5 = "invalid" }, CancellationToken.None));
        Assert.ThrowsAsync<ArgumentException>(() => voice.Transcribe(input with { MessageId = 1, FileUuid = "" }, CancellationToken.None));
        Assert.DoesNotThrow(() => voice.CompleteTranscription(123, "late unmatched result"));
        Assert.DoesNotThrow(() => voice.Disconnect());
    }

    [Test]
    public void TemporaryRoutingUsesGroupFieldThreeAndUidFieldFour()
    {
        var fixture = Convert.FromHexString("187B220175");
        var decoded = ProtoHelper.Deserialize<GroupTemp>(fixture);
        Assert.That(decoded.GroupUin, Is.EqualTo(123));
        Assert.That(decoded.ToUid, Is.EqualTo("u"));
        Assert.That(ProtoHelper.Serialize(new GroupTemp { GroupUin = 123, ToUid = "u" }).ToArray(), Is.EqualTo(fixture));
    }

    [Test]
    public void KeyboardActionReadsSparseWireFields()
    {
        var action = ProtoHelper.Deserialize<KeyboardAction>(Convert.FromHexString("180330014807"));
        Assert.That(action.ClickLimit, Is.EqualTo(3));
        Assert.That(action.AtBotShowChannelList, Is.True);
        Assert.That(action.Anchor, Is.EqualTo(7));
    }

    [Test]
    public void TranscriptionPushPreservesCorrelationFields()
    {
        // field 2 wraps item: msgId=7, text=ok, sender=8, receiver=9, uuid=u.
        var push = ProtoHelper.Deserialize<PttTransPush>(Convert.FromHexString("120D080742026F6B480850096A0175"));
        Assert.That(push.Item, Is.Not.Null);
        Assert.That(push.Item!.MsgId, Is.EqualTo(7));
        Assert.That(push.Item.Text, Is.EqualTo("ok"));
        Assert.That(push.Item.SenderUin, Is.EqualTo(8));
        Assert.That(push.Item.ReceiverUin, Is.EqualTo(9));
        Assert.That(push.Item.Uuid, Is.EqualTo("u"));
    }

    [Test]
    public void FlashFileReadsExtensionWithoutMarkdownMarker()
    {
        var element = new Elem { CommonElem = new CommonElem
        {
            ServiceType = 45,
            PbElem = Convert.FromHexString("30013A070A026673120161")
        } };
        var parsed = ((IMessageEntity)new FlashFileEntity()).Parse([element], element);
        Assert.That(parsed, Is.TypeOf<FlashFileEntity>());
        Assert.That(((FlashFileEntity)parsed!).FilesetId, Is.EqualTo("fs"));
        Assert.That(((FlashFileEntity)parsed).FileName, Is.EqualTo("a"));
    }

    [Test]
    public void FlashFileReadsLegacyEncodedRichUiCard()
    {
        const string card = """{"busId":"FlashTransfer","data":{"fileSetId":"legacy-fs","title":"archive.zip","sceneType":2}}""";
        string markdown = $"[FlashTransfer](mqqapi://richui/show?json={Uri.EscapeDataString(card)})";
        byte[] text = Encoding.UTF8.GetBytes(markdown);
        var wire = new List<byte> { 0x0A };
        uint length = (uint)text.Length;
        while (length >= 128) { wire.Add((byte)(length | 128)); length >>= 7; }
        wire.Add((byte)length);
        wire.AddRange(text);
        var element = new Elem { CommonElem = new CommonElem { ServiceType = 45, BusinessType = 1, PbElem = wire.ToArray() } };
        var parsed = ((IMessageEntity)new FlashFileEntity()).Parse([element], element);
        Assert.That(parsed, Is.TypeOf<FlashFileEntity>());
        var flash = (FlashFileEntity)parsed!;
        Assert.That(flash.FilesetId, Is.EqualTo("legacy-fs"));
        Assert.That(flash.FileName, Is.EqualTo("archive.zip"));
        Assert.That(flash.SceneType, Is.EqualTo(2));
    }

    [Test]
    public void OnlineDeviceSnapshotDistinguishesUnknownEmptyAndReset()
    {
        var cache = new CacheContext(null!);
        Assert.That(cache.OnlineDevices, Is.Null);
        cache.SetOnlineDevices([]);
        Assert.That(cache.OnlineDevices, Is.Empty);
        var original = new[] { new BotOnlineDevice(1, 2, 3, 4, "desktop") };
        cache.SetOnlineDevices(original);
        original[0] = original[0] with { DeviceName = "mutated" };
        Assert.That(cache.OnlineDevices![0].DeviceName, Is.EqualTo("desktop"));
        var snapshot = (BotOnlineDevice[])cache.OnlineDevices!;
        snapshot[0] = snapshot[0] with { DeviceName = "mutated again" };
        Assert.That(cache.OnlineDevices![0].DeviceName, Is.EqualTo("desktop"));
        cache.ResetSessionNotifications();
        Assert.That(cache.OnlineDevices, Is.Null);
    }

    [Test]
    public void CardNotificationsTrackOnlyObservedChangesAndResetPerSession()
    {
        var cache = new CacheContext(null!);
        Assert.That(cache.ObserveCard(1, 2, "first"), Is.Null);
        Assert.That(cache.ObserveCard(1, 2, "first"), Is.Null);
        Assert.That(cache.ObserveCard(3, 2, "other group"), Is.Null);
        var change = cache.ObserveCard(1, 2, "second");
        Assert.That(change, Is.Not.Null);
        Assert.That(change!.OldCard, Is.EqualTo("first"));
        Assert.That(change.NewCard, Is.EqualTo("second"));
        Assert.That(change.GroupUin, Is.EqualTo(1));
        Assert.That(change.UserUin, Is.EqualTo(2));
        cache.ResetSessionNotifications();
        Assert.That(cache.ObserveCard(1, 2, "after reset"), Is.Null);
    }

    [Test]
    public void TodoRejectsMismatchedJumpIdentity()
    {
        var banner = new OidbOnlineBanner
        {
            MsgId = Encoding.UTF8.GetBytes("7_8"),
            CommonBanner = new OidbGroupTopBannerCommon
            {
                JumpInfo = new OidbGroupTopBannerJumpInfo { JumpParam = Encoding.UTF8.GetBytes("{\"seq\":9,\"random\":8}") }
            }
        };
        Assert.Throws<OperationException>(() => ExtendedOperationExt.ParseTodo(banner));
    }
}
