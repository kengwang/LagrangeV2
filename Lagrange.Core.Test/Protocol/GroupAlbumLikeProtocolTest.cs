using Lagrange.Core.Internal.Packets.Web;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Test.Protocol;

public sealed class GroupAlbumLikeProtocolTest
{
    [Test]
    public void LikeRequestKeepsOperationAndMediaIdentity()
    {
        var request = new DoQunLikeRequest { Field1 = 5495, Body = new AlbumLikeBody { Type = 2, Like = new AlbumLikeInfo { Id = "id", Status = 0 }, Publish = new AlbumLikePublish { CellMedia = new AlbumLikeMedia { AlbumId = "album", BatchId = 7 }, CellQunInfo = new AlbumLikeQun { QunId = "123" } } } };
        var decoded = ProtoHelper.Deserialize<DoQunLikeRequest>(ProtoHelper.Serialize(request).Span);
        Assert.Multiple(() =>
        {
            Assert.That(decoded.Field1, Is.EqualTo(5495));
            Assert.That(decoded.Body.Like.Id, Is.EqualTo("id"));
            Assert.That(decoded.Body.Publish.CellMedia.BatchId, Is.EqualTo(7));
            Assert.That(decoded.Body.Publish.CellQunInfo.QunId, Is.EqualTo("123"));
        });
    }
}
