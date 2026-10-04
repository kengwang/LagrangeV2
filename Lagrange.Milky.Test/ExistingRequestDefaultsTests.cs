using System.Text.Json;
using Lagrange.Milky.Api.Handlers.File;
using Lagrange.Milky.Api.Handlers.Group;
using Lagrange.Milky.Api.Handlers.Interaction;
using Lagrange.Milky.Api.Handlers.Message;
using Lagrange.Milky.Api.Handlers.System;
using Lagrange_Milky;

namespace Lagrange.Milky.Test;

public sealed class ExistingRequestDefaultsTests
{
    private static T Read<T>(string json) where T : class
    {
        var options = new JsonSerializerOptions().AddSerializerContextsFromLagrange_Milky();
        return (T)JsonSerializer.Deserialize(json, options.GetTypeInfo(typeof(T)))!;
    }

    [Test]
    public void GeneratedJsonPreservesPrimaryConstructorDefaults()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Read<SendLikeHandler.Request>("""{"user_id":1}""").Count, Is.EqualTo(1));
            Assert.That(Read<GetCollectionHandler.Request>("{}").Count, Is.EqualTo(50));
            Assert.That(Read<SetGroupReactionHandler.Request>("""{"group_id":1,"message_seq":1,"code":"1"}""").IsSet, Is.True);
            Assert.That(Read<GetCookiesHandler.Request>("{}").Domains, Is.EqualTo(new[] { "qq.com" }));
            Assert.That(Read<GetGroupAnnouncementsHandler.Request>("""{"group_id":1}""").Start, Is.EqualTo(-1));
            Assert.That(Read<GetGroupAnnouncementsHandler.Request>("""{"group_id":1}""").Count, Is.EqualTo(20));
            Assert.That(Read<GetForwardedMessagesHandler.Request>("""{"forward_id":"f"}""").MessageScene, Is.EqualTo("friend"));
            Assert.That(Read<UploadGroupFileHandler.Request>("""{"group_id":1,"file_uri":"u","file_name":"f"}""").ParentFolderId, Is.EqualTo("/"));
            Assert.That(Read<UploadGroupAlbumImageHandler.Request>("""{"group_id":1,"album_id":"a","image_uri":"u"}""").FileName, Is.EqualTo("image.jpg"));
        });
    }

    [Test]
    public void GeneratedJsonPreservesAnnouncementInitializers()
    {
        var request = Read<SendGroupAnnouncementHandler.Request>("""{"group_id":1,"content":"test"}""");
        Assert.Multiple(() =>
        {
            Assert.That(request.ShowEditCard, Is.True);
            Assert.That(request.ConfirmRequired, Is.True);
            Assert.That(request.ImageWidth, Is.EqualTo(540));
            Assert.That(request.ImageHeight, Is.EqualTo(300));
        });
    }

    [Test]
    public void GeneratedJsonPreservesFlashCommitInitializers()
    {
        var request = Read<CommitFlashFileHandler.Request>("""{"fileset_uuid":"set","upload_key":"key","file_uuid":"file","file_name":"f","file_size":1}""");
        Assert.That(request.Index, Is.EqualTo(1));
        Assert.That(request.FormatCode, Is.EqualTo(26));
    }

    [Test]
    public void GeneratedJsonPreservesOptionalCardTextInitializers()
    {
        var request = Read<SendTuwenArkHandler.Request>("""{"peer_id":1}""");
        Assert.Multiple(() =>
        {
            Assert.That(request.Title, Is.Empty);
            Assert.That(request.Description, Is.Empty);
            Assert.That(request.Summary, Is.Empty);
            Assert.That(request.Url, Is.Empty);
            Assert.That(request.PreviewUrl, Is.Empty);
        });
    }

    [Test]
    public void ExplicitFalseZeroAndTextRemainIntact()
    {
        var announcement = Read<SendGroupAnnouncementHandler.Request>("""{"group_id":1,"content":"x","show_edit_card":false,"confirm_required":false,"image_width":0,"image_height":0}""");
        Assert.Multiple(() =>
        {
            Assert.That(announcement.ShowEditCard, Is.False);
            Assert.That(announcement.ConfirmRequired, Is.False);
            Assert.That(announcement.ImageWidth, Is.Zero);
            Assert.That(announcement.ImageHeight, Is.Zero);
            Assert.That(Read<SendTuwenArkHandler.Request>("""{"title":"title","url":"https://example.test/"}""").Title, Is.EqualTo("title"));
            Assert.That(Read<SendLikeHandler.Request>("""{"user_id":1,"count":0}""").Count, Is.Zero);
        });
    }
}
