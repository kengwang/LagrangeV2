using Lagrange.Core.Common;
using Lagrange.Core.Events;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Internal.Services.System;
using Lagrange.Core.Services;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Test.Protocol;

public class FlashTransferProtocolTest
{
    private static IEnumerable<TestCaseData> Services()
    {
        yield return new(new CreateFlashTaskService(), new CreateFlashTaskEventReq("a.png", 10, 7), 0x93cfu, 1u, 0u);
        yield return new(new CommitFlashFileService(), new CommitFlashFileEventReq("s", "s", "f", "a", 10, 1, 26), 0x93d0u, 1u, 0u);
        yield return new(new CompleteFlashTaskService(), new CompleteFlashTaskEventReq("s"), 0x93dbu, 1u, 0u);
        yield return new(new SetFlashTaskStatusService(), new SetFlashTaskStatusEventReq("s", 6), 0x93d1u, 1u, 0u);
        yield return new(new DeleteFlashFileService(), new DeleteFlashFileEventReq("s"), 0x9407u, 1u, 0u);
        yield return new(new RenameFlashFileService(), new RenameFlashFileEventReq("s", "a"), 0x9427u, 0u, 1u);
        yield return new(new SendFlashMessageService(), new SendFlashMessageEventReq(null, 123, "s"), 0x93d7u, 1u, 0u);
    }

    [TestCaseSource(nameof(Services))]
    public async Task EnvelopeMatchesSource(object serviceObject, object requestObject, uint command, uint subcommand, uint reserved)
    {
        using var context = new BotContext(new BotConfig(), new BotKeystore { Uin = 123, Uid = "u" }, new BotAppInfo());
        var service = (IService)serviceObject;
        var envelope = ProtoHelper.Deserialize<Oidb>((await service.Build((ProtocolEvent)requestObject, context)).Span);
        Assert.Multiple(() =>
        {
            Assert.That(envelope.Command, Is.EqualTo(command));
            Assert.That(envelope.Service, Is.EqualTo(subcommand));
            Assert.That(envelope.Reserved, Is.EqualTo(reserved));
            Assert.That(service.GetType().GetCustomAttributes(typeof(ServiceAttribute), false).Cast<ServiceAttribute>().Single().Command,
                Is.EqualTo($"OidbSvcTrpcTcp.0x{command:x}_{subcommand}"));
        });
    }

    [Test]
    public void CommitWireIncludesFileIdentityAndExplicitZeroFields()
    {
        // Flash transfer field layout: fileset, file, reserved, metadata, flags.
        // f7=26, f8=name, f9=name, f10=0, f11=10, f12=0, f24={}.
        const string payload = "0A01731201661800220028013001381A4201614A01615000580A6000C20100";
        var entry = new D93D0Info { FilesetUuid = "s", FileUuid = "f", Index = 1, FormatCode = 26, FileName = "a", OriginalName = "a", FileSize = 10 };
        var encoded = ProtoHelper.Serialize(entry);
        Assert.That(Convert.ToHexString(encoded.Span), Is.EqualTo(payload));
        var decoded = ProtoHelper.Deserialize<D93D0Info>(Convert.FromHexString(payload));
        Assert.Multiple(() =>
        {
            Assert.That(decoded.FilesetUuid, Is.EqualTo("s"));
            Assert.That(decoded.FileUuid, Is.EqualTo("f"));
            Assert.That(decoded.Field3, Is.EqualTo(0));
            Assert.That(decoded.Field24, Is.Not.Null);
        });
    }

    [Test]
    public async Task CreateKeepsTypeCodeSeparateFromFileType()
    {
        using var context = new BotContext(new BotConfig(), new BotKeystore { Uin = 123, Uid = "u" }, new BotAppInfo());
        var envelope = ProtoHelper.Deserialize<Oidb>((await ((IService)new CreateFlashTaskService()).Build(new CreateFlashTaskEventReq("a.png", 10, 7), context)).Span);
        var body = ProtoHelper.Deserialize<D93CFReq>(envelope.Body.Span);
        Assert.Multiple(() =>
        {
            Assert.That(body.TypeCode, Is.EqualTo(7));
            Assert.That(body.FileInfo.FileType, Is.EqualTo(1));
            Assert.That(body.FileInfo.Field16, Is.EqualTo(1));
            Assert.That(body.FileInfo.Uploader.Field4, Is.Not.Null);
        });
    }

    [Test]
    public void CommitRejectsMissingAcknowledgement()
    {
        using var context = new BotContext(new BotConfig(), new BotKeystore(), new BotAppInfo());
        var payload = ProtoHelper.Serialize(new Oidb { Body = ProtoHelper.Serialize(new D93D0Resp()) });
        Assert.ThrowsAsync<OperationException>(async () => await ((IService)new CommitFlashFileService()).Parse(payload, context));
    }

    [Test]
    public async Task CommitAcceptsCompleteAcknowledgement()
    {
        using var context = new BotContext(new BotConfig(), new BotKeystore(), new BotAppInfo());
        var payload = ProtoHelper.Serialize(new Oidb { Body = ProtoHelper.Serialize(new D93D0Resp { Field1 = 1, FilesetUuid = "s", UploadKey = "s" }) });
        Assert.That(await ((IService)new CommitFlashFileService()).Parse(payload, context), Is.TypeOf<CommitFlashFileEventResp>());
    }

    [Test]
    public void DeletePropagatesEnvelopeError()
    {
        using var context = new BotContext(new BotConfig(), new BotKeystore(), new BotAppInfo());
        var payload = ProtoHelper.Serialize(new Oidb { Result = 7, Message = "denied" });
        Assert.ThrowsAsync<OperationException>(async () => await ((IService)new DeleteFlashFileService()).Parse(payload, context));
    }
}
