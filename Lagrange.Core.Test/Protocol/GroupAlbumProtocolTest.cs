using Lagrange.Core.Internal.Packets.Web;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Test.Protocol;

public sealed class GroupAlbumProtocolTest
{
    [Test]
    public void MediaRequestRoundTripsWithTraceAndCursor()
    {
        var request = new GetMediaListRequest
        {
            ReqInfo = new GetMediaListReqInfo { GroupId = "123", AlbumId = "a", PageInfo = "next" },
            TraceId = "_trace",
            ExtMap = [new AlbumExtMapEntry { Key = "fc-appid", Value = "100" }]
        };
        var decoded = ProtoHelper.Deserialize<GetMediaListRequest>(ProtoHelper.Serialize(request).Span);
        Assert.Multiple(() =>
        {
            Assert.That(decoded.ReqInfo.AlbumId, Is.EqualTo("a"));
            Assert.That(decoded.ReqInfo.PageInfo, Is.EqualTo("next"));
            Assert.That(decoded.ExtMap.Single().Value, Is.EqualTo("100"));
        });
    }
}
