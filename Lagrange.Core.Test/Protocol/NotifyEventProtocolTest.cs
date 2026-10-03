using Lagrange.Core.Internal.Packets.Notify;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Test.Protocol;

public sealed class NotifyEventProtocolTest
{
    [Test]
    public void GroupNameChangeKeepsWireFieldTwo()
    {
        var encoded = ProtoHelper.Serialize(new GroupNameChange { Name = "abc" });
        Assert.That(Convert.ToHexString(encoded.Span), Is.EqualTo("1203616263"));
        Assert.That(ProtoHelper.Deserialize<GroupNameChange>(encoded.Span).Name, Is.EqualTo("abc"));
    }

    [Test]
    public void GroupMuteRoundTripsNestedState()
    {
        var packet = new GroupMute
        {
            GroupUin = 123,
            Data = new GroupMuteData
            {
                Timestamp = 9,
                State = new GroupMuteState { TargetUid = "uid", Duration = 5 },
            },
        };
        var encoded = ProtoHelper.Serialize(packet);
        Assert.That(Convert.ToHexString(encoded.Span), Is.EqualTo("087B2A0B08091A070A037569641005"));
        var decoded = ProtoHelper.Deserialize<GroupMute>(encoded.Span);
        Assert.That(decoded.GroupUin, Is.EqualTo(123));
        Assert.That(decoded.Data!.State!.TargetUid, Is.EqualTo("uid"));
        Assert.That(decoded.Data.State.Duration, Is.EqualTo(5));
    }

    [Test]
    public void GroupAdminUsesEnableFieldTwoAndAdminUidFieldOne()
    {
        var packet = new GroupAdmin
        {
            GroupUin = 7,
            Body = new GroupAdminBody { ExtraEnable = new GroupAdminExtra { AdminUid = "u" } },
        };
        var encoded = ProtoHelper.Serialize(packet);
        Assert.That(Convert.ToHexString(encoded.Span), Is.EqualTo("0807220512030A0175"));
        var decoded = ProtoHelper.Deserialize<GroupAdmin>(encoded.Span);
        Assert.That(decoded.Body!.ExtraEnable!.AdminUid, Is.EqualTo("u"));
    }
}
