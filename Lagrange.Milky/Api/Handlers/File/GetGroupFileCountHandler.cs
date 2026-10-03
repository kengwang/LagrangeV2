using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.File;

[ApiHandler("get_group_file_count")]
public sealed class GetGroupFileCountHandler(BotContext lagrange) : IApiHandler<GetGroupFileCountHandler.Request, GetGroupFileCountHandler.Result>
{
    private readonly BotContext _lagrange = lagrange;

    public async ValueTask<MilkyApiResponse<Result>> HandleAsync(Request request, CancellationToken ct)
    {
        var count = await _lagrange.FetchGroupFSCount(request.GroupId, ct);
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
