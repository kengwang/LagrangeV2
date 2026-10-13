using Lagrange.Core.Common;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Internal.Packets.Service.Migration;
using Lagrange.Core.Internal.Services.System;
using Lagrange.Core.Services;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Test.Protocol;

public sealed class DatabaseKeyTest
{
    private static BotContext Bot() => BotFactory.Create(new BotConfig { GetOptimumServer = false },
        new BotKeystore(), BotAppInfo.ProtocolToAppInfo[Protocols.Linux]);

    [TestCase(false)]
    [TestCase(true)]
    public async Task EncryptBuildsCapturedBodyWithoutAdditionalAuthentication(bool hasTicket)
    {
        using var bot = Bot();
        if (hasTicket) bot.Keystore.WLoginSigs.A2 = Enumerable.Range(0, 72).Select(x => (byte)x).ToArray();
        IService service = new RequestEncryptKeyService();
        var frame = await service.Build(new RequestEncryptKeyEventReq(), bot);
        var envelope = ProtoHelper.Deserialize<Oidb>(frame.Span);
        Assert.Multiple(() =>
        {
            Assert.That(envelope.Command, Is.EqualTo(0xcde));
            Assert.That(envelope.Service, Is.EqualTo(1));
            Assert.That(Convert.ToHexString(envelope.Body.Span), Is.EqualTo("0A025000"));
            Assert.That(Convert.ToHexString(frame.Span), Is.EqualTo("08DE191001180022040A0250002A006000"));
            Assert.That(envelope.Reserved, Is.Zero);
        });
        var registered = await bot.ServiceContext.Resolve(new RequestEncryptKeyEventReq());
        Assert.That(registered.Item1.Command, Is.EqualTo("OidbSvcTrpcTcp.0xcde_1"));
    }

    [Test]
    public async Task DecryptPreservesAsciiMetadataAndExistingWireLayout()
    {
        using var bot = Bot();
        string meta = string.Concat(Enumerable.Repeat("aB", 64));
        var request = new RequestDatabaseKeyEventReq(new Oidb0xcdeReq { Info = new() { Db_salt = meta } });
        var registered = await bot.ServiceContext.Resolve(request);
        var envelope = ProtoHelper.Deserialize<Oidb>(registered.Item1.Data.Span);
        Assert.Multiple(() =>
        {
            Assert.That(registered.Item1.Command, Is.EqualTo("OidbSvcTrpcTcp.0xcde_2"));
            Assert.That(envelope.Command, Is.EqualTo(0xcde));
            Assert.That(envelope.Service, Is.EqualTo(2));
            Assert.That(Convert.ToHexString(envelope.Body.Span), Is.EqualTo("1283010A8001" + Convert.ToHexString(System.Text.Encoding.ASCII.GetBytes(meta)) + "5200"));
            Assert.That(ProtoHelper.Deserialize<Oidb0xcdeReq>(envelope.Body.Span).Info!.Db_salt, Is.EqualTo(meta));
        });
    }

    [Test]
    public async Task BothResponseLayoutsDecodeAndSkipUnrelatedEnvelopeFields()
    {
        IService encrypt = new RequestEncryptKeyService();
        var response = (RequestEncryptKeyEventResp)await encrypt.Parse(Convert.FromHexString("08DE1910011800220D0A0B0A036B657912046D6574615A00"), null!);
        Assert.That(response.Result, Is.EqualTo(new BotDatabaseKeyResult("key", "meta")));
        IService decrypt = new RequestDatabaseKeyService();
        var existing = (RequestDatabaseKeyEventResp)await decrypt.Parse(Convert.FromHexString("08DE1910021800220712050A036B65795A00"), null!);
        Assert.That(existing.Body.Info!.DbKey, Is.EqualTo("key"));
        Assert.That(existing.Body.Info.KeyMeta, Is.Empty);
    }

    [TestCase("2200")]
    [TestCase("22020A00")]
    [TestCase("22070A050A036B6579")]
    [TestCase("22070A051203616263")]
    public void EncryptRequiresBothKeyAndMetadata(string response)
    {
        IService service = new RequestEncryptKeyService();
        Assert.ThrowsAsync<OperationException>(async () => await service.Parse(Convert.FromHexString(response), null!));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void OidbErrorsPreserveServerCodeAndMessage(bool encrypt)
    {
        using var bot = Bot();
        IService service = encrypt ? new RequestEncryptKeyService() : new RequestDatabaseKeyService();
        var error = Assert.ThrowsAsync<OperationException>(async () => await service.Parse(
            ProtoHelper.Serialize(new Oidb { Result = 1006, Message = "database mismatch" }), bot));
        Assert.That(error!.Result, Is.EqualTo(1006));
        Assert.That(error.ErrMsg, Is.EqualTo("database mismatch"));
    }

    [Test]
    public void PublicMethodsValidateBeforeSendingAndKeepLegacyContract()
    {
        using var bot = Bot();
        foreach (string? meta in new string?[] { null, "", new('a', 127), new('g', 128), new string('a', 128) + " " })
        {
            Assert.ThrowsAsync<ArgumentException>(async () => await bot.RequestDecryptKey(DatabaseKeyMode.Decrypt, meta));
            Assert.ThrowsAsync<ArgumentException>(async () => await bot.RequestDatabaseKey(meta!));
        }
        Assert.ThrowsAsync<ArgumentException>(async () => await bot.RequestDecryptKey(DatabaseKeyMode.Encrypt, ""));
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await bot.RequestDecryptKey((DatabaseKeyMode)99));
        using var canceled = new CancellationTokenSource();
        canceled.Cancel();
        Assert.ThrowsAsync<OperationCanceledException>(async () => await bot.RequestDecryptKey(DatabaseKeyMode.Decrypt, new('a', 128), canceled.Token));
    }

    [TestCase("OidbSvcTrpcTcp.0xcde_1")]
    [TestCase("OidbSvcTrpcTcp.0xcde_2")]
    public void PcSignerIncludesBothCommands(string command)
    {
        using var signer = new DefaultBotSignProvider();
        Assert.That(signer.IsWhiteListCommand(command), Is.True);
    }
}
