using System;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;
using Lagrange.Milky.Converters;

namespace Lagrange.Milky.Api.Handlers.System;

[ApiHandler("add_custom_face")]
public sealed class AddCustomFaceHandler(BotContext lagrange, ResourceConverter resources) : IApiHandler<AddCustomFaceHandler.Request, AddCustomFaceHandler.Result>
{
    public async ValueTask<MilkyApiResponse<Result>> HandleAsync(Request request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ImageUri)) throw new ArgumentException("Image URI is required.", nameof(request));
        using var image = await resources.UriToStreamAsync(request.ImageUri, ct);
        var faceId = await lagrange.AddCustomFace(image, ct).WaitAsync(ct);
        return new(new Result(faceId));
    }

    public sealed class Request(string imageUri) { [JsonPropertyName("image_uri")] public required string ImageUri { get; init; } = imageUri; }
    public sealed class Result(string faceId) { [JsonPropertyName("face_id")] public string FaceId { get; } = faceId; }
}
