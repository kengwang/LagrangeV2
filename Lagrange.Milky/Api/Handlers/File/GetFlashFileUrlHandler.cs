using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.File;

public sealed class GetFlashFileUrlHandler(BotContext lagrange) : Endpoint<GetFlashFileUrlHandler.Request, MilkyApiResponse<GetFlashFileUrlHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/get_flash_file_url");
    }
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        string url = await lagrange.GetFlashFileUrl(request.FilesetUuid, request.FileId, ct).WaitAsync(ct);
        return new(new Result { DownloadUrl = url });
    }
    public sealed class Request(string filesetUuid, string? fileId)
    {
        [JsonPropertyName("fileset_uuid")] public required string FilesetUuid { get; init; } = filesetUuid;
        [JsonPropertyName("file_id")] public string? FileId { get; init; } = fileId;
    }
    public sealed class Result { [JsonPropertyName("download_url")] public required string DownloadUrl { get; init; } }
}
