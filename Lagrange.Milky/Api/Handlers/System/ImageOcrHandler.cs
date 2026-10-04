using FastEndpoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.System;

public sealed class ImageOcrHandler(BotContext lagrange) : Endpoint<ImageOcrHandler.Request, MilkyApiResponse<ImageOcrHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/image_ocr");
    }
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ImageUrl);
        var result = await lagrange.ImageOcr(request.ImageUrl, ct).WaitAsync(ct);
        return new(new Result(result));
    }
    public sealed class Request { [JsonPropertyName("image_url")] public required string ImageUrl { get; init; } }
    public sealed class Result(Lagrange.Core.Common.Response.BotOcrResult result)
    {
        [JsonPropertyName("language")] public string Language { get; } = result.Language;
        [JsonPropertyName("texts")] public IReadOnlyList<Text> Texts { get; } = [.. result.Texts.Select(x => new Text(x))];
    }
    public sealed class Text(Lagrange.Core.Common.Response.BotOcrText text)
    {
        [JsonPropertyName("text")] public string Value { get; } = text.Text;
        [JsonPropertyName("confidence")] public uint Confidence { get; } = text.Confidence;
        [JsonPropertyName("coordinates")] public IReadOnlyList<Coordinate> Coordinates { get; } = [.. text.Coordinates.Select(x => new Coordinate(x))];
    }
    public sealed class Coordinate(Lagrange.Core.Common.Response.BotOcrCoordinate coordinate)
    {
        [JsonPropertyName("x")] public int X { get; } = coordinate.X;
        [JsonPropertyName("y")] public int Y { get; } = coordinate.Y;
    }
}
