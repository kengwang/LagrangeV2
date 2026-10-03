using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.File;

[ApiHandler("get_flash_file_url")]
public sealed class GetFlashFileUrlHandler(BotContext lagrange) : IApiHandler<GetFlashFileUrlHandler.Request, GetFlashFileUrlHandler.Result>
{
    public async ValueTask<MilkyApiResponse<Result>> HandleAsync(Request request, CancellationToken ct)
    {
        var url = await lagrange.GetFlashFileUrl(request.FilesetUuid, request.FileId, ct).WaitAsync(ct);
        return new(new Result { DownloadUrl = url });
    }
    public sealed class Request(string filesetUuid, string? fileId)
    {
        [JsonPropertyName("fileset_uuid")] public required string FilesetUuid { get; init; } = filesetUuid;
        [JsonPropertyName("file_id")] public string? FileId { get; init; } = fileId;
    }
    public sealed class Result { [JsonPropertyName("download_url")] public required string DownloadUrl { get; init; } }
}
