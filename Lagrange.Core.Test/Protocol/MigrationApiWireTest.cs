using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Internal.Packets.Service.Migration;
using Lagrange.Core.Internal.Services.System;
using Lagrange.Core.Services;
using Lagrange.Core.Utility;
namespace Lagrange.Core.Test.Protocol;
public sealed class MigrationApiWireTest
{
    [Test]
    public async Task ButtonUsesProtocolSparseFieldsAndCommand()
    {
        IService service = new ClickKeyboardService();
        var request = new ClickKeyboardEventReq(new() { BotAppid = 1, MsgSeq = 2, ButtonId = "x", CallbackData = "y", GroupId = 3, Unknown9 = 1 });
        var envelope = ProtoHelper.Deserialize<Oidb>((await service.Build(request, null!)).Span);
        Assert.That(envelope.Command, Is.EqualTo(0x112e)); Assert.That(envelope.Service, Is.EqualTo(1));
        Assert.That(Convert.ToHexString(envelope.Body.Span), Is.EqualTo("180120022A0178320179380040034801"));
        var response = (ClickKeyboardEventResp)await service.Parse(Convert.FromHexString("22091807220268692A0178"), null!);
        Assert.That(response.Body.Result, Is.EqualTo(7)); Assert.That(response.Body.PromptText, Is.EqualTo("hi"));
    }
    [Test]
    public void KeyAndMiniAppResponseUseNestedWireFields()
    {
        Assert.That(ProtoHelper.Deserialize<Oidb0xcdeResp>(Convert.FromHexString("12050A036B6579")).Info!.DbKey, Is.EqualTo("key"));
        var card = ProtoHelper.Deserialize<MiniAppShareResp>(Convert.FromHexString("1007220412027B7D"));
        Assert.That(card.Status, Is.EqualTo(7)); Assert.That(card.Body!.JsonStr, Is.EqualTo("{}"));
    }
    [Test]
    public async Task UserStatusQueriesProperty27372InUinForm()
    {
        IService service = new FetchStrangerService();
        var envelope = ProtoHelper.Deserialize<Oidb>((await service.Build(new GetUserStatusEventReq(new() { Uin = 123, Key = [new() { Key = 27372 }] }), null!)).Span);
        Assert.That(envelope.Command, Is.EqualTo(0xfe1)); Assert.That(envelope.Service, Is.EqualTo(2)); Assert.That(envelope.Reserved, Is.EqualTo(1));
        Assert.That(Convert.ToHexString(envelope.Body.Span), Is.EqualTo("087B1A0408ECD501"));
    }
    [TestCase(false)]
    [TestCase(true)]
    public async Task TodoMutationsCarryExactMessageSequence(bool remove)
    {
        IService service = remove ? new GroupRemoveTodoService() : new GroupFinishTodoService();
        var bytes = remove ? await service.Build(new GroupRemoveTodoEventReq(123, 456), null!) : await service.Build(new GroupFinishTodoEventReq(123, 456), null!);
        var envelope = ProtoHelper.Deserialize<Oidb>(bytes.Span);
        Assert.That(envelope.Command, Is.EqualTo(0xf90)); Assert.That(envelope.Service, Is.EqualTo(remove ? 3 : 2));
        Assert.That(Convert.ToHexString(envelope.Body.Span), Is.EqualTo("087B10C80318002000"));
    }
    [Test]
    public async Task RKeyRequestsAllScopesAndPropagatesInnerError()
    {
        IService service = new DownloadRKeyService();
        var envelope = ProtoHelper.Deserialize<Oidb>((await service.Build(new DownloadRKeyEventReq(), null!)).Span);
        Assert.That(envelope.Command, Is.EqualTo(0x9067)); Assert.That(envelope.Service, Is.EqualTo(202)); Assert.That(envelope.Reserved, Is.EqualTo(1));
        var request = ProtoHelper.Deserialize<NTV2RichMediaReq>(envelope.Body.Span);
        Assert.That(request.DownloadRKey.Types, Is.EqualTo(new[] { 10u, 20u, 2u }));
        Assert.That(request.ReqHead.Scene.BusinessType, Is.EqualTo(1));
        // OIDB field4 body -> response field1 head -> field2 code42, field3 "bad".
        Assert.ThrowsAsync<OperationException>(async () => await service.Parse(Convert.FromHexString("22090A07102A1A03626164"), null!));
    }
}
