using System;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using Lagrange.Codec;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Common.Response;
using Lagrange.Milky.Converters;

namespace Lagrange.Milky.Api.Handlers.Group;

/// <summary>Uploads a video and caller-supplied cover to a group album.</summary>
/// <param name="lagrange">The authenticated bot.</param>
/// <param name="resources">Resolves upload resource URIs.</param>
public sealed class UploadGroupAlbumVideoHandler(BotContext lagrange, ResourceConverter resources) : Endpoint<UploadGroupAlbumVideoHandler.Request, MilkyApiResponse<UploadGroupAlbumVideoHandler.Result>>
{
    private readonly BotContext _lagrange = lagrange ?? throw new ArgumentNullException(nameof(lagrange));
    private readonly ResourceConverter _resources = resources ?? throw new ArgumentNullException(nameof(resources));

    /// <inheritdoc />
    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/upload_group_album_video");
    }

    /// <inheritdoc />
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        if (request.GroupId <= 0) throw new ArgumentOutOfRangeException(nameof(request.GroupId));
        ArgumentException.ThrowIfNullOrWhiteSpace(request.AlbumId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.VideoUri);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.CoverUri);
        if (request.DurationMilliseconds is <= 0) throw new ArgumentOutOfRangeException(nameof(request.DurationMilliseconds));
        using var video = await _resources.UriToStreamAsync(request.VideoUri, ct).ConfigureAwait(false);
        using var cover = await _resources.UriToStreamAsync(request.CoverUri, ct).ConfigureAwait(false);
        if (request.DurationMilliseconds is { } duration)
            return new(new Result(await _lagrange.UploadGroupAlbumVideo(request.GroupId, request.AlbumId, request.AlbumName,
                video, cover, duration, request.FileName, ct).ConfigureAwait(false)));

        // Native video_get_size accepts a byte buffer and reports whole seconds.
        using var bufferedVideo = new MemoryStream();
        byte[] chunk = new byte[81920];
        int count;
        while ((count = await video.ReadAsync(chunk, ct).ConfigureAwait(false)) != 0)
        {
            if (bufferedVideo.Length + count > 1536L * 1024 * 1024) throw new ArgumentException("Video exceeds the 1.5 GiB upload limit.");
            await bufferedVideo.WriteAsync(chunk.AsMemory(0, count), ct).ConfigureAwait(false);
        }
        byte[] bytes = bufferedVideo.ToArray();
        if (bytes.Length < 8 || !bytes.AsSpan(4, 4).SequenceEqual("ftyp"u8)) throw new ArgumentException("Video must use an ISO BMFF container.");
        var info = await Task.Run(() => VideoCodec.GetSize(bytes), ct).ConfigureAwait(false);
        ct.ThrowIfCancellationRequested();
        if (info.Duration <= 0) throw new ArgumentException("The native codec cannot determine a positive whole-second duration; supply measured duration_ms explicitly.");
        var result = await _lagrange.UploadGroupAlbumVideo(request.GroupId, request.AlbumId, request.AlbumName, bufferedVideo,
            cover, checked(info.Duration * 1000), request.FileName, ct).ConfigureAwait(false);
        return new(new Result(result));
    }

    /// <summary>Group album video upload parameters.</summary>
    public sealed class Request
    {
        private string? _albumName;
        private string? _fileName;
        /// <summary>Destination group number.</summary>
        [JsonPropertyName("group_id")] public required long GroupId { get; init; }
        /// <summary>Destination album id.</summary>
        [JsonPropertyName("album_id")] public required string AlbumId { get; init; }
        /// <summary>Existing album name.</summary>
        [JsonPropertyName("album_name")] public string AlbumName { get => _albumName ?? ""; init => _albumName = value; }
        /// <summary>Resource URI of an ISO BMFF video, such as MP4.</summary>
        [JsonPropertyName("video_uri")] public required string VideoUri { get; init; }
        /// <summary>Resource URI of the video's cover image.</summary>
        [JsonPropertyName("cover_uri")] public required string CoverUri { get; init; }
        /// <summary>Measured duration in milliseconds; omission uses the native codec's whole-second duration.</summary>
        [JsonPropertyName("duration_ms")] public long? DurationMilliseconds { get; init; }
        /// <summary>Video display name.</summary>
        [JsonPropertyName("file_name")] public string FileName { get => _fileName ?? "video.mp4"; init => _fileName = value; }
    }

    /// <summary>Server-confirmed upload identifiers.</summary>
    /// <param name="result">Completed video and cover upload.</param>
    public sealed class Result(BotGroupAlbumVideoUploadResult result)
    {
        /// <summary>Video identifier.</summary>
        [JsonPropertyName("video_id")] public string VideoId { get; } = result.VideoId;
        /// <summary>Destination album identifier.</summary>
        [JsonPropertyName("album_id")] public string AlbumId { get; } = result.AlbumId;
        /// <summary>Cover photo identifier.</summary>
        [JsonPropertyName("cover_id")] public string CoverId { get; } = result.CoverId;
        /// <summary>Cover URL.</summary>
        [JsonPropertyName("cover_url")] public string CoverUrl { get; } = result.CoverUrl;
    }
}
