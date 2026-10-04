using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.File;

public sealed class GetGroupFileCountHandler(BotContext lagrange) : Endpoint<GetGroupFileCountHandler.Request, MilkyApiResponse<GetGroupFileCountHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/get_group_file_count");
    }
    private readonly BotContext _lagrange = lagrange;

    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        uint count = await _lagrange.FetchGroupFSCount(request.GroupId, ct);
        return new MilkyApiResponse<Result>(new Result { FileCount = count });
    }

    public sealed class Request(long groupId)
    {
        [JsonPropertyName("group_id")] public required long GroupId { get; init; } = groupId;
    }

    public sealed class Result
    {
        [JsonPropertyName("file_count")] public required uint FileCount { get; init; }
    }
}
