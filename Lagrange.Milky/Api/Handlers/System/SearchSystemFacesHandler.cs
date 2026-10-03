using System.Collections.Generic;
using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.System;

[ApiHandler("search_system_faces")]
public sealed class SearchSystemFacesHandler(BotContext lagrange) : IApiHandler<SearchSystemFacesHandler.Request, SearchSystemFacesHandler.Result>
{
    private readonly BotContext _lagrange = lagrange;
    public async ValueTask<MilkyApiResponse<Result>> HandleAsync(Request request, CancellationToken ct)
    {
        var packs = await _lagrange.GetSystemFaces(request.Refresh, ct);
        var faces = packs.SelectMany(pack => pack.Faces).Where(face => face.Sid.Contains(request.Query, StringComparison.OrdinalIgnoreCase) || face.Description.Contains(request.Query, StringComparison.OrdinalIgnoreCase) || face.Aliases.Any(alias => alias.Contains(request.Query, StringComparison.OrdinalIgnoreCase))).Select(face => new SearchFace { Sid = face.Sid, Description = face.Description, EmCode = face.EmCode, Url = face.Url }).ToList();
        return new MilkyApiResponse<Result>(new Result { Faces = faces });
    }
    public sealed class Request(string query, bool refresh = false) { [JsonPropertyName("query")] public required string Query { get; init; } = query; [JsonPropertyName("refresh")] public bool Refresh { get; init; } = refresh; }
    public sealed class Result { [JsonPropertyName("faces")] public required IReadOnlyList<SearchFace> Faces { get; init; } }
    public sealed class SearchFace { [JsonPropertyName("sid")] public required string Sid { get; init; } [JsonPropertyName("description")] public required string Description { get; init; } [JsonPropertyName("em_code")] public required string EmCode { get; init; } [JsonPropertyName("url")] public string? Url { get; init; } }
}
