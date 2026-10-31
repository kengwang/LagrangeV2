using Lagrange.Core.Internal.Packets.Web;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Test.Protocol;

public sealed class GroupAlbumCommentProtocolTest
{
    [Test]
    public void CommentRequestKeepsMediaAndContentFields()
    {
        var media = new AlbumCommentPhotoInfo { AlbumId = "a", BatchId = ulong.MaxValue, Media = [new AlbumMediaInfo { Image = new AlbumImageInfo { Lloc = "lloc" } }] };
        byte[] header = Convert.FromHexString("08A603187B2204666565642A0463656C6C3003");
        var request = new DoQunCommentRequest { Field1 = 8527, Body = new DoQunCommentBody { GroupId = "123", Field3 = 2,
            ReqBody = new AlbumCommentReqBody { Header = header, PhotoInfo = ProtoHelper.Serialize(media).ToArray() },
            Content = new AlbumCommentContent { User = new AlbumCommentUser { Uin = "456" }, Contents = [new AlbumCommentText { Content = "hello" }] } } };
        var decoded = ProtoHelper.Deserialize<DoQunCommentRequest>(ProtoHelper.Serialize(request).Span);
        Assert.Multiple(() =>
        {
            Assert.That(decoded.Field1, Is.EqualTo(8527));
            Assert.That(decoded.Body.ReqBody.Header, Is.EqualTo(header));
            var photo = ProtoHelper.Deserialize<AlbumCommentPhotoInfo>(decoded.Body.ReqBody.PhotoInfo);
            Assert.That(photo.AlbumId, Is.EqualTo("a"));
            Assert.That(photo.Media.Single().Image!.Lloc, Is.EqualTo("lloc"));
            Assert.That(photo.BatchId, Is.EqualTo(ulong.MaxValue));
            Assert.That(decoded.Body.Content.Contents.Single().Content, Is.EqualTo("hello"));
            Assert.That(Convert.ToHexString(ProtoHelper.Serialize(decoded.Body.Content.Contents.Single()).Span), Is.EqualTo("0800120568656C6C6F"));
        });
    }
}
