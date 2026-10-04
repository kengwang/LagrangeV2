using FastEndpoints;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.System;

public sealed class GetCustomFaceListHandler(BotContext lagrange) : EndpointWithoutRequest<MilkyApiResponse<GetCustomFaceListHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/get_custom_face_list");
    }
    private readonly BotContext _lagrange = lagrange;

    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(CancellationToken ct)
    {
        var result = await _lagrange.GetCustomFaceList(cancellationToken: ct);
        return new MilkyApiResponse<Result>(new Result { FaceIds = result.FaceIds, TotalCount = result.TotalCount });
    }

    public sealed class Result
    {
        [JsonPropertyName("face_ids")] public required IReadOnlyList<string> FaceIds { get; init; }
        [JsonPropertyName("total_count")] public uint TotalCount { get; init; }
    }
}
