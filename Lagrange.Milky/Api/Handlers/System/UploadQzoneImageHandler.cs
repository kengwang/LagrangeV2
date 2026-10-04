using FastEndpoints;
using System;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Converters;

namespace Lagrange.Milky.Api.Handlers.System;

public sealed class UploadQzoneImageHandler(BotContext lagrange, ResourceConverter resources) : Endpoint<UploadQzoneImageHandler.Request, MilkyApiResponse<UploadQzoneImageHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/upload_qzone_image");
    }
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ImageUri)) throw new ArgumentException("Image URI is required.", nameof(request));
        using var image = await resources.UriToStreamAsync(request.ImageUri, ct);
        return new(new Result(await lagrange.UploadQzoneImage(image, ct).WaitAsync(ct)));
    }
    public sealed class Request(string imageUri) { [JsonPropertyName("image_uri")] public string ImageUri { get; init; } = imageUri; }
    public sealed class Result(Lagrange.Core.Common.Response.BotQzoneUploadResult result)
    {
        [JsonPropertyName("rich_value")] public string RichValue { get; } = result.RichValue;
        [JsonPropertyName("url")] public string Url { get; } = result.Url;
        [JsonPropertyName("album_id")] public string AlbumId { get; } = result.AlbumId;
        [JsonPropertyName("location")] public string Location { get; } = result.Location;
        [JsonPropertyName("type")] public int Type { get; } = result.Type;
        [JsonPropertyName("width")] public int Width { get; } = result.Width;
        [JsonPropertyName("height")] public int Height { get; } = result.Height;
    }
}
