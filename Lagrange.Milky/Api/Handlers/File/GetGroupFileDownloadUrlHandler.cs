using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.File;

public sealed class GetGroupFileDownloadUrlHandler(BotContext lagrange) : Endpoint<GetGroupFileDownloadUrlHandler.Request, MilkyApiResponse<GetGroupFileDownloadUrlHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/get_group_file_download_url");
    }
    private readonly BotContext _lagrange = lagrange;

    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        string url = await _lagrange.GroupFSDownload(request.GroupId, request.FileId, ct);
        return new MilkyApiResponse<Result>(new Result
        {
            DownloadUrl = url,
        });
    }

    public sealed class Request(long groupId, string fileId)
    {
        [JsonPropertyName("group_id")] public required long GroupId { get; init; } = groupId;
        [JsonPropertyName("file_id")] public required string FileId { get; init; } = fileId;
    }

    public sealed class Result
    {
        [JsonPropertyName("download_url")] public required string DownloadUrl { get; init; }
    }
}
