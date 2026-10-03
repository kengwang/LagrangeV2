using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Test.Protocol;

public sealed class ReadReportProtocolTest
{
    [Test]
    public void GroupReadReportUsesProtocolFieldNumbers()
    {
        var request = new SsoReadedReportReq { GroupList = [new GroupReadedReportItem { GroupUin = 123, LastReadSeq = 456 }] };
        var decoded = ProtoHelper.Deserialize<SsoReadedReportReq>(ProtoHelper.Serialize(request).Span);
        Assert.That(decoded.GroupList!.Single().LastReadSeq, Is.EqualTo(456));
    }

    [Test]
    public void PrivateReadReportUsesUidFieldTwo()
    {
        var request = new SsoReadedReportReq { C2CList = [new C2CReadedReportItem { Uid = "uid", LastReadSeq = 9 }] };
        var encoded = ProtoHelper.Serialize(request);
        Assert.That(Convert.ToHexString(encoded.Span), Does.StartWith("1209"));
        var decoded = ProtoHelper.Deserialize<SsoReadedReportReq>(encoded.Span);
        Assert.That(decoded.C2CList!.Single().Uid, Is.EqualTo("uid"));
    }
}
