using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;

namespace Lagrange.Core.Internal.Http;

internal sealed class GroupAlbumVideoUploader(Func<Uri, HttpContent, CancellationToken, Task<JsonDocument>> send)
{
    private const long MaxVideoBytes = 1536L * 1024 * 1024;
    private const long MaxCoverBytes = 32L * 1024 * 1024;
    private const string BaseUrl = "https://h5.qzone.qq.com/webapp/json/sliceUpload/";

    internal async Task<BotGroupAlbumVideoUploadResult> Upload(long botUin, UploadGroupAlbumVideoEventReq request, CancellationToken ct)
    {
        if (request.GroupUin <= 0) throw new ArgumentOutOfRangeException(nameof(request.GroupUin));
        ArgumentException.ThrowIfNullOrWhiteSpace(request.AlbumId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.FileName);
        if (request.DurationMilliseconds <= 0) throw new ArgumentOutOfRangeException(nameof(request.DurationMilliseconds));
        await using var video = await UploadSource.Open(request.Video, MaxVideoBytes, ct).ConfigureAwait(false);
        await using var cover = await UploadSource.Open(request.Cover, MaxCoverBytes, ct).ConfigureAwait(false);
        byte[] head = new byte[16];
        int length = (int)Math.Min(head.Length, video.Stream.Length);
        await video.Stream.ReadExactlyAsync(head.AsMemory(0, length), ct).ConfigureAwait(false);
        if (length < 8 || !head.AsSpan(4, 4).SequenceEqual("ftyp"u8)) throw new ArgumentException("Group album video must use an ISO BMFF container, such as MP4.");
        video.Stream.Position = 0;
        length = (int)Math.Min(head.Length, cover.Stream.Length);
        await cover.Stream.ReadExactlyAsync(head.AsMemory(0, length), ct).ConfigureAwait(false);
        if (!IsImage(head.AsSpan(0, length))) throw new ArgumentException("Video cover must be JPEG, PNG, GIF, WebP or BMP.");
        cover.Stream.Position = 0;
        string sha1 = Convert.ToHexString(await SHA1.HashDataAsync(video.Stream, ct).ConfigureAwait(false)).ToLowerInvariant();
        string md5 = Convert.ToHexString(await MD5.HashDataAsync(cover.Stream, ct).ConfigureAwait(false)).ToLowerInvariant();
        video.Stream.Position = cover.Stream.Position = 0;
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var videoControl = BuildControl(botUin, request, sha1, video.Stream.Length, timestamp, timestamp, null);
        var videoSession = await CreateSession(sha1, videoControl, ct).ConfigureAwait(false);
        var videoResult = await UploadSlices(botUin, video.Stream, videoSession, true, ct).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(videoResult.VideoId)) throw new OperationException(-1, "Video upload did not return a video id.");

        var coverControl = BuildControl(botUin, request, md5, cover.Stream.Length, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), timestamp, videoResult.VideoId);
        var coverSession = await CreateSession(md5, coverControl, ct).ConfigureAwait(false);
        var coverResult = await UploadSlices(botUin, cover.Stream, coverSession, false, ct).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(coverResult.PhotoId) || string.IsNullOrWhiteSpace(coverResult.PhotoUrl))
            throw new OperationException(-1, "Video cover upload did not return its photo id and URL.");
        return new() { VideoId = videoResult.VideoId, AlbumId = request.AlbumId, CoverId = coverResult.PhotoId, CoverUrl = coverResult.PhotoUrl };
    }

    internal static JsonObject BuildControl(long botUin, UploadGroupAlbumVideoEventReq request, string checksum, long size, long timestamp, long batchTimestamp, string? videoId)
    {
        bool video = videoId == null;
        var biz = new JsonObject
        {
            ["sPicTitle"] = request.FileName, ["sPicDesc"] = "", ["sAlbumName"] = video ? "" : request.AlbumName,
            ["sAlbumID"] = video ? "" : request.AlbumId, ["iAlbumTypeID"] = 0, ["iBitmap"] = 0,
            ["iUploadType"] = video ? 3 : 2, ["iUpPicType"] = 0, ["iBatchID"] = video ? 0 : batchTimestamp,
            ["sPicPath"] = "", ["iPicWidth"] = 0, ["iPicHight"] = 0, ["iWaterType"] = 0, ["iDistinctUse"] = 0,
            ["iUploadTime"] = timestamp
        };
        if (video)
        {
            biz["sTitle"] = request.FileName;
            biz["sDesc"] = "";
            biz["iFlag"] = 0;
            biz["iPlayTime"] = request.DurationMilliseconds;
            biz["sCoverUrl"] = "";
            biz["iIsNew"] = 111;
            biz["iIsOriginalVideo"] = 0;
            biz["iIsFormatF20"] = 0;
            biz["extend_info"] = new JsonObject { ["video_type"] = "3", ["domainid"] = "5", ["photo_num"] = "0", ["video_num"] = "1", ["qun_id"] = Invariant(request.GroupUin) };
        }
        else
        {
            biz["iNeedFeeds"] = 1;
            biz["mapExt"] = new JsonObject { ["appid"] = "qun", ["userid"] = Invariant(request.GroupUin) };
            biz["mutliPicInfo"] = new JsonObject { ["iBatUploadNum"] = 1, ["iCurUpload"] = 0, ["iSuccNum"] = 0, ["iFailNum"] = 0 };
            biz["stExtendInfo"] = new JsonObject { ["mapParams"] = new JsonObject { ["vid"] = videoId, ["photo_num"] = "0", ["video_num"] = "1" } };
            biz["stExternalMapExt"] = new JsonObject { ["is_client_upload_cover"] = "1", ["is_pic_video_mix_feeds"] = "1" };
            foreach (string key in new[] { "sExif_CameraMaker", "sExif_CameraModel", "sExif_Time", "sExif_LatitudeRef", "sExif_Latitude", "sExif_LongitudeRef", "sExif_Longitude" }) biz[key] = "";
        }
        return new JsonObject
        {
            ["control_req"] = new JsonArray(new JsonObject
            {
                ["uin"] = Invariant(botUin), ["token"] = new JsonObject { ["type"] = 4, ["data"] = "", ["appid"] = 5 },
                ["appid"] = video ? "video_qun" : "qun", ["checksum"] = checksum, ["check_type"] = video ? 1 : 0,
                ["file_len"] = size, ["env"] = new JsonObject { ["refer"] = video ? "qzone" : "huodong", ["deviceInfo"] = "h5" },
                ["model"] = 0, ["biz_req"] = biz, ["session"] = "", ["asy_upload"] = 0, ["cmd"] = video ? "FileUploadVideo" : ""
            })
        };
    }

    private async Task<Session> CreateSession(string checksum, JsonObject control, CancellationToken ct)
    {
        using var content = new StringContent(control.ToJsonString(), Encoding.UTF8, "application/json");
        using var response = await send(new Uri(BaseUrl + "FileBatchControl/" + checksum), content, ct).ConfigureAwait(false);
        JsonElement data = RequireSuccess(response.RootElement);
        string? session = Text(data, "session");
        if (string.IsNullOrWhiteSpace(session)) throw new OperationException(-1, "Album upload session is missing.");
        int sliceSize = data.TryGetProperty("slice_size", out var size) && size.TryGetInt32(out int value) && value > 0 ? value : 16384;
        if (sliceSize > 16 * 1024 * 1024) throw new OperationException(-1, "Album upload slice size exceeds the supported buffer limit.");
        return new(session, sliceSize);
    }

    private async Task<UploadResult> UploadSlices(long botUin, Stream stream, Session session, bool video, CancellationToken ct)
    {
        byte[] buffer = new byte[session.SliceSize];
        long offset = 0;
        int sequence = 0;
        string? videoId = null, photoId = null, photoUrl = null;
        while (offset < stream.Length)
        {
            ct.ThrowIfCancellationRequested();
            int length = (int)Math.Min(buffer.Length, stream.Length - offset);
            await stream.ReadExactlyAsync(buffer.AsMemory(0, length), ct).ConfigureAwait(false);
            long end = offset + length;
            using var form = CreateSliceForm(botUin, session.Id, video, offset, sequence, buffer, length);
            string operation = video ? "FileUploadVideo" : "FileUpload";
            var uri = new Uri($"{BaseUrl}{operation}?seq={sequence}&retry=0&offset={offset}&end={end}&total={stream.Length}&type=form");
            using var response = await send(uri, form, ct).ConfigureAwait(false);
            var data = RequireSuccess(response.RootElement);
            if (data.ValueKind == JsonValueKind.Object && data.TryGetProperty("biz", out var biz))
            {
                videoId = Text(biz, "sVid") ?? videoId;
                photoId = Text(biz, "sPhotoID") ?? photoId;
                photoUrl = Text(biz, "sBURL") ?? photoUrl;
            }
            offset = end;
            sequence++;
        }
        return new(videoId, photoId, photoUrl);
    }

    internal static MultipartFormDataContent CreateSliceForm(long botUin, string session, bool video, long offset, int sequence, byte[] buffer, int length)
    {
        // Match the browser FormData expected by the album uploader: quoted names,
        // plain filename, unquoted boundary, and no content type on text fields.
        string boundary = $"----formdata-lagrange-{Guid.NewGuid():N}";
        var form = new MultipartFormDataContent(boundary);
        form.Headers.ContentType!.Parameters.Single(parameter => parameter.Name == "boundary").Value = boundary;
        void Add(string key, string value)
        {
            var field = new StringContent(value);
            field.Headers.ContentType = null;
            field.Headers.ContentDisposition = new("form-data") { Name = $"\"{key}\"" };
            form.Add(field);
        }
        Add("uin", Invariant(botUin));
        Add("appid", video ? "video_qun" : "qun");
        Add("session", session);
        Add("offset", Invariant(offset));
        var bytes = new ByteArrayContent(buffer, 0, length);
        bytes.Headers.ContentType = new("application/octet-stream");
        bytes.Headers.ContentDisposition = new("form-data") { Name = "\"data\"", FileName = "\"blob\"" };
        form.Add(bytes);
        Add("checksum", "");
        Add("check_type", video ? "1" : "0");
        Add("retry", "0");
        Add("seq", Invariant(sequence));
        Add("end", Invariant(offset + length));
        Add("cmd", "FileUpload");
        Add("slice_size", Invariant(length));
        return form;
    }

    private static JsonElement RequireSuccess(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("ret", out var result) || result.ValueKind != JsonValueKind.Number || !result.TryGetInt32(out int code))
            throw new OperationException(-1, "Album upload response is missing its result code.");
        if (code != 0) throw new OperationException(code, Text(root, "msg") ?? "Album upload failed.");
        return root.TryGetProperty("data", out var data) ? data : default;
    }

    private static string? Text(JsonElement value, string property) => value.ValueKind == JsonValueKind.Object && value.TryGetProperty(property, out var item) && item.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(item.GetString()) ? item.GetString() : null;
    private static string Invariant(long number) => number.ToString(CultureInfo.InvariantCulture);
    private static bool IsImage(ReadOnlySpan<byte> bytes) => bytes.StartsWith(new byte[] { 0xff, 0xd8 }) || bytes.StartsWith(new byte[] { 0x89, 0x50, 0x4e, 0x47 })
        || bytes.StartsWith("GIF8"u8) || bytes.StartsWith("BM"u8) || (bytes.Length >= 12 && bytes.StartsWith("RIFF"u8) && bytes.Slice(8, 4).SequenceEqual("WEBP"u8));
    private sealed record Session(string Id, int SliceSize);
    private sealed record UploadResult(string? VideoId, string? PhotoId, string? PhotoUrl);

    private sealed class UploadSource(Stream stream, bool ownsStream, long position) : IAsyncDisposable
    {
        public Stream Stream { get; } = stream;

        internal static async Task<UploadSource> Open(Stream input, long limit, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(input);
            if (!input.CanRead) throw new ArgumentException("Upload stream must be readable.", nameof(input));
            ct.ThrowIfCancellationRequested();
            if (input.CanSeek)
            {
                if (input.Length is <= 0 || input.Length > limit) throw new ArgumentException("Upload input is empty or exceeds its size limit.", nameof(input));
                long position = input.Position;
                input.Position = 0;
                return new(input, false, position);
            }
            var staged = new FileStream(Path.Combine(Path.GetTempPath(), $"lagrange-album-{Guid.NewGuid():N}.tmp"), FileMode.CreateNew,
                FileAccess.ReadWrite, FileShare.None, 81920, FileOptions.Asynchronous | FileOptions.DeleteOnClose);
            try
            {
                byte[] buffer = new byte[81920];
                int count;
                while ((count = await input.ReadAsync(buffer, ct).ConfigureAwait(false)) != 0)
                {
                    if (staged.Length + count > limit) throw new ArgumentException("Upload input exceeds its size limit.", nameof(input));
                    await staged.WriteAsync(buffer.AsMemory(0, count), ct).ConfigureAwait(false);
                }
                if (staged.Length == 0) throw new ArgumentException("Upload input is empty.", nameof(input));
                staged.Position = 0;
                return new(staged, true, 0);
            }
            catch
            {
                await staged.DisposeAsync().ConfigureAwait(false);
                throw;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (ownsStream) await Stream.DisposeAsync().ConfigureAwait(false);
            else Stream.Position = position;
        }
    }
}
