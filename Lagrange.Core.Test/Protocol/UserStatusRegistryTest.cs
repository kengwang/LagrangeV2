using System.Text;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Internal.Packets.Service.Migration;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Test.Protocol;

public sealed class UserStatusRegistryTest
{
    private static BotContext CreateContext() => new(new BotConfig(), new BotKeystore { Uin = 1, Uid = "self" }, new BotAppInfo());

    [Test]
    public async Task ActualRegistryRoutesStatusAndProfileRequestsThroughOneService()
    {
        using var context = CreateContext();
        var (statusPacket, _) = await context.ServiceContext.Resolve(new GetUserStatusEventReq(new OidbStrangerStatusReq
        {
            Uin = 123, Key = [new() { Key = 27372 }]
        }));
        Assert.That(statusPacket.Command, Is.EqualTo("OidbSvcTrpcTcp.0xfe1_2"));
        var statusEnvelope = ProtoHelper.Deserialize<Oidb>(statusPacket.Data.Span);
        Assert.That(statusEnvelope.Reserved, Is.EqualTo(1));
        var statusRequest = ProtoHelper.Deserialize<OidbStrangerStatusReq>(statusEnvelope.Body.Span);
        Assert.That(statusRequest.Key.Single().Key, Is.EqualTo(27372));
        Assert.That(statusRequest.Uin, Is.EqualTo(123));

        var (profilePacket, _) = await context.ServiceContext.Resolve(new FetchStrangerByUinEventReq(123));
        var profileEnvelope = ProtoHelper.Deserialize<Oidb>(profilePacket.Data.Span);
        Assert.That(ProtoHelper.Deserialize<FetchStrangerByUinRequest>(profileEnvelope.Body.Span).Keys.Select(x => x.Key), Does.Contain(20002UL));

        var responseWire = ProtoHelper.Serialize(new Oidb
        {
            Command = 0xfe1, Service = 2, Body = Convert.FromHexString("0A0C12080A0608ECD5011001187B")
        });
        var result = await context.ServiceContext.Resolve(new BotSsoPacket(statusPacket.Command, responseWire));
        Assert.That(result, Is.InstanceOf<GetUserStatusEventResp>());
        var value = ((GetUserStatusEventResp)result).Body.Data!.Properties!.Entries.Single().Value;
        Assert.That(ExtendedOperationExt.DecodeUserStatus(value).Status, Is.EqualTo(10));
    }

    [Test]
    public async Task ActualRegistryParsesProfileWithoutLosingExistingFields()
    {
        using var context = CreateContext();
        var properties = new FetchStrangerResponseProperties
        {
            NumberProperties = new ulong[] { 105, 20009, 20026, 20037 }.Select(x => new FetchStrangerResponseNumberProperties { Key = x, Value = 1 }).ToList(),
            BytesProperties = new ulong[] { 102, 103, 27394, 20003, 20004 }.Select(x => new FetchStrangerResponseBytesProperties { Key = x, Value = [] }).ToList()
        };
        properties.BytesProperties.Add(new() { Key = 20002, Value = Encoding.UTF8.GetBytes("profile") });
        properties.BytesProperties.Add(new() { Key = 20031, Value = [0, 0, 0, 0] });
        var response = new FetchStrangerResponse { Body = new() { Uin = 123, Properties = properties } };
        var packet = new BotSsoPacket("OidbSvcTrpcTcp.0xfe1_2", ProtoHelper.Serialize(new Oidb { Command = 0xfe1, Service = 2, Body = ProtoHelper.Serialize(response) }));
        var result = (FetchStrangerEventResp)await context.ServiceContext.Resolve(packet);
        Assert.That(result.Stranger.Uin, Is.EqualTo(123));
        Assert.That(result.Stranger.Nickname, Is.EqualTo("profile"));
    }
}
