using System.Text;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Internal.Services.System;
using Lagrange.Core.Services;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Test.Protocol;

public sealed class FriendsStatusTest
{
    private const string SelfUid = "u_abcdefghijklmnopqrstuv";
    private static BotContext Bot() => BotFactory.Create(new BotConfig { GetOptimumServer = false },
        new BotKeystore { Uin = 123456, Uid = SelfUid }, BotAppInfo.ProtocolToAppInfo[Protocols.Linux]);

    [Test]
    public async Task RequestMatchesCapturedFieldsAndUsesSessionUid()
    {
        using var bot = Bot();
        var (packet, _) = await bot.ServiceContext.Resolve(new GetFriendsStatusEventReq());
        var envelope = ProtoHelper.Deserialize<Oidb>(packet.Data.Span);
        Assert.Multiple(() =>
        {
            Assert.That(packet.Command, Is.EqualTo("OidbSvcTrpcTcp.0x116d_1"));
            Assert.That(envelope.Command, Is.EqualTo(0x116d));
            Assert.That(envelope.Service, Is.EqualTo(1));
            Assert.That(envelope.Reserved, Is.Zero);
            Assert.That(Convert.ToHexString(envelope.Body.Span), Is.EqualTo("0A18" + Convert.ToHexString(Encoding.UTF8.GetBytes(SelfUid)) + "28013800400148005000A00600A8069003"));
        });
    }

    [TestCase("")]
    [TestCase(" ")]
    public void MissingSessionUidFailsBeforeSending(string uid)
    {
        using var bot = Bot();
        bot.Keystore.Uid = uid;
        IService service = new GetFriendsStatusService();
        Assert.ThrowsAsync<InvalidOperationException>(async () => await service.Build(new GetFriendsStatusEventReq(), bot));
        Assert.That(bot.PacketContext.PendingCount, Is.Zero);
    }

    [Test]
    public async Task CapturedEntryShapeRetainsDevicesSelfDuplicatesAndFieldOrder()
    {
        // Synthesized UIDs; remaining fields have the captured computer/mobile layouts.
        byte[] computer = [0x0a, 0x03, (byte)'u', (byte)'_', (byte)'a', .. Convert.FromHexString("100A1801300A5081820470057A0CE794B5E88491E59CA8E7BABF9201009A0100AA01020800")];
        byte[] mobile = [0x0a, 0x18, .. Encoding.ASCII.GetBytes(SelfUid), .. Convert.FromHexString("100A1801300B50878204581460016805700C7A0CE6898BE69CBAE59CA8E7BABF9201009A0100AA010608C391E0AB06")];
        var result = await Parse(computer, mobile, computer);
        Assert.That(result.SelfUid, Is.EqualTo(SelfUid));
        Assert.That(result.Friends.Select(friend => friend.Uid), Is.EqualTo(new[] { "u_a", SelfUid, "u_a" }));
        Assert.That(result.Friends[0].Status, Is.EqualTo(10));
        Assert.That(result.Friends[0].StatusText, Is.EqualTo("电脑在线"));
        Assert.That(result.Friends[1].StatusText, Is.EqualTo("手机在线"));
        Assert.That(result.Friends[0].RawFields, Is.EqualTo(new BotFriendStatusRawField[]
        {
            new(3, 0, "1"), new(6, 0, "10"), new(10, 0, "65793"), new(14, 0, "5"),
            new(18, 2, ""), new(19, 2, ""), new(21, 2, "0800")
        }));
        Assert.That(result.Friends[1].RawFields.Last(), Is.EqualTo(new BotFriendStatusRawField(21, 2, "08C391E0AB06")));
    }

    [Test]
    public async Task MissingStatusIsNullAndRawFieldsPreserveFullWidthsAndDuplicates()
    {
        var result = await Parse(Convert.FromHexString("0A03755F6138FFFFFFFFFFFFFFFFFF0138054100010203040506FF4DABCD0012"));
        var friend = result.Friends.Single();
        Assert.That(friend.Status, Is.Null);
        Assert.That(friend.StatusText, Is.Empty);
        Assert.That(friend.RawFields, Is.EqualTo(new BotFriendStatusRawField[]
        {
            new(7, 0, "18446744073709551615"), new(7, 0, "5"),
            new(8, 1, "00010203040506FF"), new(9, 5, "ABCD0012")
        }));
    }

    [Test]
    public async Task EmptyListIsAValidSingleResponse()
    {
        var result = await Parse();
        Assert.That(result.Friends, Is.Empty);
        Assert.That(result.SelfUid, Is.EqualTo(SelfUid));
    }

    [TestCase("")]
    [TestCase("0A00")]
    [TestCase("00")]
    [TestCase("80")]
    [TestCase("FFFFFFFFFF01")]
    [TestCase("0A03755F613880")]
    [TestCase("0A03755F6138FFFFFFFFFFFFFFFFFF02")]
    [TestCase("0A03755F6138FFFFFFFFFFFFFFFFFF81")]
    [TestCase("0A03755F61410102")]
    [TestCase("0A03755F614D0102")]
    [TestCase("0A03755F614A05AA")]
    [TestCase("0A03755F614AFFFFFFFF0F")]
    [TestCase("0A03755F613B")]
    [TestCase("0A03755F613E")]
    [TestCase("0801")]
    [TestCase("0A03755F611200")]
    [TestCase("0A03755F61108080808010")]
    [TestCase("0A03755F617800")]
    [TestCase("0A03755F617A01FF")]
    public void MalformedEntryFailsInsteadOfReturningPartialData(string entry)
    {
        Assert.That(async () => await Parse(Convert.FromHexString(entry)), Throws.Exception);
    }

    [Test]
    public void OidbFailureRetainsServerError()
    {
        using var bot = Bot();
        IService service = new GetFriendsStatusService();
        var error = Assert.ThrowsAsync<OperationException>(async () => await service.Parse(
            ProtoHelper.Serialize(new Oidb { Result = 42, Message = "status unavailable" }), bot));
        Assert.That(error!.Result, Is.EqualTo(42));
        Assert.That(error.ErrMsg, Is.EqualTo("status unavailable"));
    }

    [Test]
    public void CancellationIsPropagatedBeforeSending()
    {
        using var bot = Bot();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        Assert.ThrowsAsync<OperationCanceledException>(async () => await bot.GetFriendsStatus(cancellation.Token));
        Assert.That(bot.PacketContext.PendingCount, Is.Zero);
    }

    [Test]
    public void ExistingSignersAlreadyIncludeCommand()
    {
        using var pc = new DefaultBotSignProvider();
        using var android = new DefaultAndroidBotSignProvider();
        Assert.That(pc.IsWhiteListCommand("OidbSvcTrpcTcp.0x116d_1"), Is.True);
        Assert.That(android.IsWhiteListCommand("OidbSvcTrpcTcp.0x116d_1"), Is.True);
    }

    private static async Task<BotFriendsStatusResult> Parse(params byte[][] entries)
    {
        IService service = new GetFriendsStatusService();
        var body = ProtoHelper.Serialize(new FriendsStatusResponse
        {
            SelfUid = SelfUid, Friends = entries.Select(entry => (ReadOnlyMemory<byte>)entry).ToList()
        });
        var envelope = ProtoHelper.Serialize(new Oidb { Command = 0x116d, Service = 1, Body = body });
        // Real replies also contain unrelated OIDB metadata (field11); leave it uninterpreted.
        byte[] frame = [.. envelope.Span, 0x5a, 0x00];
        var response = (GetFriendsStatusEventResp)await service.Parse(frame, null!);
        return response.Result;
    }
}
