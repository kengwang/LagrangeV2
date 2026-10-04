using FastEndpoints;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Internal.Events.System;

namespace Lagrange.Milky.Api.Handlers.System;

public sealed class GetCustomFaceDetailHandler(BotContext lagrange) : Endpoint<GetCustomFaceDetailHandler.Request, MilkyApiResponse<GetCustomFaceDetailHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/get_custom_face_detail");
    }
    private readonly BotContext _lagrange = lagrange;

    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        var result = await _lagrange.GetCustomFaceDetail(request.Entries.Select(entry => new CustomFaceLookup(entry.FaceId, entry.Md5)).ToArray(), ct);
        return new MilkyApiResponse<Result>(new Result
        {
            Entries = [.. result.Entries.Select(entry => new Detail { FaceId = entry.FaceId, Description = entry.Description })],
        });
    }

    public sealed class Request(IReadOnlyList<FaceEntry> entries)
    {
        [JsonPropertyName("entries")] public required IReadOnlyList<FaceEntry> Entries { get; init; } = entries;
    }

    public sealed class FaceEntry
    {
        [JsonPropertyName("face_id")] public required string FaceId { get; init; }
        [JsonPropertyName("md5")] public required string Md5 { get; init; }
    }

    public sealed class Result
    {
        [JsonPropertyName("entries")] public required IReadOnlyList<Detail> Entries { get; init; }
    }

    public sealed class Detail
    {
        [JsonPropertyName("face_id")] public required string FaceId { get; init; }
        [JsonPropertyName("description")] public required string Description { get; init; }
    }
}
