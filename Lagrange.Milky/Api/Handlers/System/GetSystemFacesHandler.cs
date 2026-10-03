using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.System;

[ApiHandler("get_system_faces")]
public sealed class GetSystemFacesHandler(BotContext lagrange) : IApiHandler<GetSystemFacesHandler.Request, GetSystemFacesHandler.Result>
{
    private readonly BotContext _lagrange = lagrange;
    public async ValueTask<MilkyApiResponse<Result>> HandleAsync(Request request, CancellationToken ct)
    {
        var packs = await _lagrange.GetSystemFaces(request.Refresh, ct);
        return new MilkyApiResponse<Result>(new Result { Packs = [.. packs.Select(pack => new Pack { PackName = pack.PackName, Faces = [.. pack.Faces.Select(face => new SystemFace { Sid = face.Sid, Description = face.Description, EmCode = face.EmCode, CategoryId = face.CategoryId, Url = face.Url, Aliases = face.Aliases })] })] });
    }
    public sealed class Request(bool refresh = false) { [JsonPropertyName("refresh")] public bool Refresh { get; init; } = refresh; }
    public sealed class Result { [JsonPropertyName("packs")] public required IReadOnlyList<Pack> Packs { get; init; } }
    public sealed class Pack { [JsonPropertyName("pack_name")] public required string PackName { get; init; } [JsonPropertyName("faces")] public required IReadOnlyList<SystemFace> Faces { get; init; } }
    public sealed class SystemFace { [JsonPropertyName("sid")] public required string Sid { get; init; } [JsonPropertyName("description")] public required string Description { get; init; } [JsonPropertyName("em_code")] public required string EmCode { get; init; } [JsonPropertyName("category_id")] public int? CategoryId { get; init; } [JsonPropertyName("url")] public string? Url { get; init; } [JsonPropertyName("aliases")] public IReadOnlyList<string> Aliases { get; init; } = []; }
}
