using Lagrange.Core.Internal.Packets.Web;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Test.Protocol;

public sealed class GroupAlbumCommentProtocolTest
{
    [Test]
    public void CommentRequestKeepsMediaAndContentFields()
    {
        var request = new DoQunCommentRequest { Field1 = 8527, Body = new DoQunCommentBody { GroupId = "123", Field3 = 2, ReqBody = new AlbumCommentReqBody { PhotoInfo = new AlbumCommentPhotoInfo { AlbumId = "a", Wrap = new AlbumCommentPhotoWrap { Meta = new AlbumCommentPhotoMeta { Lloc = "lloc" } } } }, Content = new AlbumCommentContent { User = new AlbumCommentUser { Uin = "456" }, Meta = new AlbumCommentContentMeta { Content = "hello" } } } };
        var decoded = ProtoHelper.Deserialize<DoQunCommentRequest>(ProtoHelper.Serialize(request).Span);
        Assert.Multiple(() =>
        {
            Assert.That(decoded.Field1, Is.EqualTo(8527));
            Assert.That(decoded.Body.ReqBody.PhotoInfo.AlbumId, Is.EqualTo("a"));
            Assert.That(decoded.Body.ReqBody.PhotoInfo.Wrap.Meta.Lloc, Is.EqualTo("lloc"));
            Assert.That(decoded.Body.Content.Meta.Content, Is.EqualTo("hello"));
        });
    }
}
