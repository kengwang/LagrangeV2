using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.File;

[ApiHandler("get_group_file_space")]
public sealed class GetGroupFileSpaceHandler(BotContext lagrange) : IApiHandler<GetGroupFileSpaceHandler.Request, GetGroupFileSpaceHandler.Result>
{
    private readonly BotContext _lagrange = lagrange;

    public async ValueTask<MilkyApiResponse<Result>> HandleAsync(Request request, CancellationToken ct)
    {
        var space = await _lagrange.FetchGroupFSSpaceInfo(request.GroupId, ct);
        return new MilkyApiResponse<Result>(new Result
        {
            UsedSpace = space.UsedSpace,
            TotalSpace = space.TotalSpace,
            FreeSpace = space.TotalSpace >= space.UsedSpace ? space.TotalSpace - space.UsedSpace : 0,
        });
    }

    public sealed class Request(long groupId)
    {
        [JsonPropertyName("group_id")] public required long GroupId { get; init; } = groupId;
    }

    public sealed class Result
    {
        [JsonPropertyName("used_space")] public required ulong UsedSpace { get; init; }
        [JsonPropertyName("total_space")] public required ulong TotalSpace { get; init; }
        [JsonPropertyName("free_space")] public required ulong FreeSpace { get; init; }
    }
}
