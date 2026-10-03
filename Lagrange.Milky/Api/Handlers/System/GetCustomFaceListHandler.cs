using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.System;

[ApiHandler("get_custom_face_list")]
public sealed class GetCustomFaceListHandler(BotContext lagrange) : INoRequestApiHandler<GetCustomFaceListHandler.Result>
{
    private readonly BotContext _lagrange = lagrange;

    public async ValueTask<MilkyApiResponse<Result>> HandleAsync(CancellationToken ct)
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
