using FastEndpoints;
using System;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Converters;

namespace Lagrange.Milky.Api.Handlers.Group;

public sealed class UploadGroupAlbumImageHandler(BotContext lagrange, ResourceConverter resources) : Endpoint<UploadGroupAlbumImageHandler.Request, MilkyApiResponse<UploadGroupAlbumImageHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/upload_group_album_image");
    }
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        if (request.GroupId <= 0) throw new ArgumentOutOfRangeException(nameof(request.GroupId));
        if (string.IsNullOrWhiteSpace(request.AlbumId)) throw new ArgumentException("Album id is required.", nameof(request.AlbumId));
        using var image = await resources.UriToStreamAsync(request.ImageUri, ct);
        var result = await lagrange.UploadGroupAlbumImage(request.GroupId, request.AlbumId, request.AlbumName, image, request.FileName, ct).WaitAsync(ct);
        return new(new Result(result));
    }

    public sealed class Request(long groupId, string albumId, string imageUri, string albumName = "", string fileName = "image.jpg")
    {
        [JsonPropertyName("group_id")] public long GroupId { get; init; } = groupId;
        [JsonPropertyName("album_id")] public string AlbumId { get; init; } = albumId;
        [JsonPropertyName("image_uri")] public string ImageUri { get; init; } = imageUri;
        [JsonPropertyName("album_name")] public string AlbumName { get; init; } = albumName;
        [JsonPropertyName("file_name")] public string FileName { get; init; } = fileName;
    }

    public sealed class Result(Lagrange.Core.Common.Response.BotGroupAlbumUploadResult result)
    {
        [JsonPropertyName("media_id")] public string MediaId { get; } = result.MediaId;
        [JsonPropertyName("album_id")] public string AlbumId { get; } = result.AlbumId;
        [JsonPropertyName("url")] public string Url { get; } = result.Url;
    }
}
