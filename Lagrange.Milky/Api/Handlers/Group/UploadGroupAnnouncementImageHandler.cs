using FastEndpoints;
using System;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Common.Response;
using Lagrange.Milky.Converters;

namespace Lagrange.Milky.Api.Handlers.Group;

public sealed class UploadGroupAnnouncementImageHandler(BotContext lagrange, ResourceConverter resources) : Endpoint<UploadGroupAnnouncementImageHandler.Request, MilkyApiResponse<UploadGroupAnnouncementImageHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/upload_group_announcement_image");
    }
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ImageUri)) throw new ArgumentException("Image URI is required.", nameof(request));
        using var image = await resources.UriToStreamAsync(request.ImageUri, ct);
        var result = await lagrange.UploadGroupAnnouncementImage(image, ct);
        return new(new Result(result));
    }

    public sealed class Request(string imageUri)
    {
        [JsonPropertyName("image_uri")] public required string ImageUri { get; init; } = imageUri;
    }

    public sealed class Result(BotGroupAnnouncementImage image)
    {
        [JsonPropertyName("id")] public string Id { get; } = image.Id;
        [JsonPropertyName("width")] public int Width { get; } = image.Width;
        [JsonPropertyName("height")] public int Height { get; } = image.Height;
    }
}
