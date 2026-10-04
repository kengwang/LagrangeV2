using System.Text.Json;
using System.Text.Json.Nodes;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Http;
using Lagrange.Core.Internal.Packets.Web;
using Lagrange.Core.Internal.Services.Message;
using Lagrange.Core.Services;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Test.Protocol;

public sealed class GroupAlbumMigrationTest
{
    [Test]
    public async Task AlbumListUsesProtocolEnvelopeAndCursorFieldTwo()
    {
        IService service = new GetGroupAlbumsService();
        var bytes = await service.Build(new GetGroupAlbumsEventReq(12345, "next"), null!);
        Assert.That(Convert.ToHexString(bytes.Span[..7]), Is.EqualTo("08831A12001A00"));
        var request = ProtoHelper.Deserialize<GetAlbumListRequest>(bytes.Span);
        Assert.That(request.Data.GroupId, Is.EqualTo("12345"));
        Assert.That(request.Data.AttachInfo, Is.EqualTo("next"));
        Assert.That(request.ExtMap.Single().Key, Is.EqualTo("fc-appid"));
        Assert.That(request.ExtMap.Single().Value, Is.EqualTo("100"));
    }

    [Test]
    public async Task AlbumListReadsWirePaginationAndSurfacesServerErrors()
    {
        // Independent protobuf fixture: envelope.data { album { id=a, name=x, create=1, modify=2, last=3, count=4 }, cursor=n, more=true }.
        byte[] fixture = Convert.FromHexString("22150A0E0A01611A0178280130023803400412016E1801");
        IService service = new GetGroupAlbumsService();
        var response = (GetGroupAlbumsEventResp)await service.Parse(fixture, null!);
        Assert.Multiple(() =>
        {
            Assert.That(response.Result.Albums.Single().Name, Is.EqualTo("x"));
            Assert.That(response.Result.Albums.Single().LastUploadTime, Is.EqualTo(3));
            Assert.That(response.Result.Albums.Single().UploadNumber, Is.EqualTo(4));
            Assert.That(response.Result.AttachInfo, Is.EqualTo("n"));
            Assert.That(response.Result.HasMore, Is.True);
        });
        Assert.ThrowsAsync<OperationException>(async () => await service.Parse(Convert.FromHexString("10051A03626164"), null!));
    }

    [Test]
    public async Task AlbumPaginationAdvancesAndRejectsCycles()
    {
        int calls = 0;
        var albums = await GroupAlbumExt.ReadAllPages((cursor, _) =>
        {
            calls++;
            Assert.That(cursor, Is.EqualTo(calls == 1 ? "" : "next"));
            return Task.FromResult(new BotGroupAlbumResult { Albums = [new() { AlbumId = calls.ToString() }], HasMore = calls == 1, AttachInfo = calls == 1 ? "next" : "" });
        }, CancellationToken.None);
        Assert.That(albums, Has.Count.EqualTo(2));
        calls = 0;
        Assert.ThrowsAsync<OperationException>(async () => await GroupAlbumExt.ReadAllPages((_, _) => Task.FromResult(new BotGroupAlbumResult
        {
            Albums = [], HasMore = true, AttachInfo = ++calls == 2 ? "b" : "a"
        }), CancellationToken.None));
        Assert.That(calls, Is.EqualTo(3));
        Assert.Throws<OperationException>(() => GroupAlbumExt.ValidatePage(new() { Albums = [], HasMore = true }, ""));
    }

    private static UploadGroupAlbumVideoEventReq Request(Stream video, Stream cover) => new(123, "album", "name", video, cover, 2500, "video.mp4");
    private static MemoryStream Video() => new([0, 0, 0, 16, .. "ftypisom"u8, 0, 0, 0, 0]);
    private static MemoryStream Cover() => new([0x89, 0x50, 0x4e, 0x47, 13, 10, 26, 10]);

    [Test]
    public async Task VideoSlicesUseBrowserFormDataHeadersAndOrdering()
    {
        using var form = GroupAlbumVideoUploader.CreateSliceForm(123, "test-session", true, 6, 1, [1, 2, 3, 4], 4);
        string boundary = form.Headers.ContentType!.Parameters.Single(parameter => parameter.Name == "boundary").Value!;
        string wire = await form.ReadAsStringAsync();
        Assert.Multiple(() =>
        {
            Assert.That(boundary, Does.Not.Contain("\""));
            Assert.That(wire, Does.StartWith($"--{boundary}\r\nContent-Disposition: form-data; name=\"uin\"\r\n\r\n123\r\n"));
            Assert.That(wire, Does.Not.Contain("filename*="));
            Assert.That(wire, Does.Not.Contain("text/plain"));
            Assert.That(wire, Does.Contain("name=\"data\"; filename=\"blob\""));
            Assert.That(wire, Does.Contain("Content-Type: application/octet-stream"));
            string[] fieldNames = ["uin", "appid", "session", "offset", "data", "checksum", "check_type", "retry", "seq", "end", "cmd", "slice_size"];
            int previous = -1;
            foreach (string fieldName in fieldNames)
            {
                int current = wire.IndexOf($"name=\"{fieldName}\"", StringComparison.Ordinal);
                Assert.That(current, Is.GreaterThan(previous), $"Serialized field {fieldName} should appear in FormData order.");
                previous = current;
            }
        });
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    public async Task VideoMediaIsIdentifiedByItsPayloadAcrossTypeVariants(byte type)
    {
        // Envelope.data.media { type, video { id=v, width=16, height=9 } }.
        byte[] fixture = [0x22, 0x0d, 0x1a, 0x0b, 0x08, type, 0x1a, 0x07, 0x0a, 0x01, 0x76, 0x20, 0x10, 0x28, 0x09];
        IService service = new GetGroupAlbumMediaService();
        var result = (GetGroupAlbumMediaEventResp)await service.Parse(fixture, null!);
        var video = result.Result.Media.Single();
        Assert.Multiple(() =>
        {
            Assert.That(video.Type, Is.EqualTo("video"));
            Assert.That(video.Id, Is.EqualTo("v"));
            Assert.That(video.Width, Is.EqualTo(16));
            Assert.That(video.Height, Is.EqualTo(9));
        });
    }

    [Test]
    public async Task VideoUploadSendsBothSessionsAndAccurateFinalSliceLengths()
    {
        var controls = new List<JsonNode>();
        var slices = new List<(string Path, string AppId, string CheckType, string Length, int Bytes, string Offset)>();
        var uploader = new GroupAlbumVideoUploader(async (uri, content, ct) =>
        {
            if (uri.AbsolutePath.Contains("FileBatchControl"))
            {
                controls.Add(JsonNode.Parse(await content.ReadAsStringAsync(ct))!);
                return JsonDocument.Parse("""{"ret":0,"data":{"session":"s","slice_size":6}}""");
            }
            var form = (MultipartFormDataContent)content;
            var fields = new Dictionary<string, string>();
            int byteCount = 0;
            foreach (var part in form)
            {
                string key = part.Headers.ContentDisposition!.Name!.Trim('"');
                if (key == "data") byteCount = (await part.ReadAsByteArrayAsync(ct)).Length;
                else fields[key] = await part.ReadAsStringAsync(ct);
            }
            slices.Add((uri.AbsolutePath, fields["appid"], fields["check_type"], fields["slice_size"], byteCount, fields["offset"]));
            Assert.That(fields["cmd"], Is.EqualTo("FileUpload"));
            return JsonDocument.Parse(uri.AbsolutePath.EndsWith("FileUploadVideo")
                ? """{"ret":0,"data":{"biz":{"sVid":"vid"}}}"""
                : """{"ret":0,"data":{"biz":{"sPhotoID":"cover","sBURL":"https://example.test/cover"}}}""");
        });
        using var video = Video();
        using var cover = Cover();
        video.Position = 2;
        var result = await uploader.Upload(456, Request(video, cover), CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result.VideoId, Is.EqualTo("vid"));
            Assert.That(result.CoverId, Is.EqualTo("cover"));
            Assert.That(controls, Has.Count.EqualTo(2));
            Assert.That(controls[0]["control_req"]![0]!["appid"]!.GetValue<string>(), Is.EqualTo("video_qun"));
            Assert.That(controls[0]["control_req"]![0]!["biz_req"]!["iPlayTime"]!.GetValue<long>(), Is.EqualTo(2500));
            Assert.That(controls[1]["control_req"]![0]!["biz_req"]!["stExtendInfo"]!["mapParams"]!["vid"]!.GetValue<string>(), Is.EqualTo("vid"));
            Assert.That(controls[1]["control_req"]![0]!["biz_req"]!["sAlbumID"]!.GetValue<string>(), Is.EqualTo("album"));
            Assert.That(slices.Select(x => x.Bytes), Is.EqualTo(new[] { 6, 6, 4, 6, 2 }));
            Assert.That(slices.Select(x => x.Length), Is.EqualTo(new[] { "6", "6", "4", "6", "2" }));
            Assert.That(slices.Select(x => x.Offset), Is.EqualTo(new[] { "0", "6", "12", "0", "6" }));
            Assert.That(video.Position, Is.EqualTo(2));
            Assert.That(video.CanRead, Is.True);
            Assert.That(cover.CanRead, Is.True);
        });
    }

    [Test]
    public void RejectedVideoSliceDoesNotUploadCoverOrClaimSuccess()
    {
        int calls = 0;
        var uploader = new GroupAlbumVideoUploader((_, _, _) => Task.FromResult(JsonDocument.Parse(++calls == 1
            ? """{"ret":0,"data":{"session":"s","slice_size":6}}"""
            : """{"ret":7,"msg":"rejected"}""")));
        using var video = Video();
        using var cover = Cover();
        Assert.ThrowsAsync<OperationException>(async () => await uploader.Upload(456, Request(video, cover), CancellationToken.None));
        Assert.That(calls, Is.EqualTo(2));
        Assert.That(video.Position, Is.Zero);
    }

    [Test]
    public void CancelledUploadSendsNoControlRequests()
    {
        int calls = 0;
        var uploader = new GroupAlbumVideoUploader((_, _, _) => { calls++; throw new InvalidOperationException(); });
        using var video = Video();
        using var cover = Cover();
        Assert.ThrowsAsync<OperationCanceledException>(async () => await uploader.Upload(456, Request(video, cover), new CancellationToken(true)));
        Assert.That(calls, Is.Zero);
    }
}
