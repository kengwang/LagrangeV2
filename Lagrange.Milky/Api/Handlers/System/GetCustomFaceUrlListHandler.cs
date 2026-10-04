using FastEndpoints;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.System;

public sealed class GetCustomFaceUrlListHandler(BotContext lagrange) : EndpointWithoutRequest<MilkyApiResponse<GetCustomFaceUrlListHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/get_custom_face_url_list");
    }
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(CancellationToken ct)
    {
        var result = await lagrange.GetCustomFaceList(cancellationToken: ct);
        var urls = new List<string>(result.FaceIds.Count);
        foreach (string id in result.FaceIds) urls.Add($"https://p.qpic.cn/qq_expression/{lagrange.BotUin}/{id}/0");
        return new(new Result { FaceIds = result.FaceIds, Urls = urls });
    }
    public sealed class Result
    {
        [JsonPropertyName("face_ids")] public required IReadOnlyList<string> FaceIds { get; init; }
        [JsonPropertyName("urls")] public required IReadOnlyList<string> Urls { get; init; }
    }
}
