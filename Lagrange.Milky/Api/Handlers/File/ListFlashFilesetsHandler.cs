using FastEndpoints;
using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Common.Response;

namespace Lagrange.Milky.Api.Handlers.File;

public sealed class ListFlashFilesetsHandler(BotContext lagrange) : Endpoint<ListFlashFilesetsHandler.Request, MilkyApiResponse<ListFlashFilesetsHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/list_flash_filesets");
    }
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        var result = await lagrange.ListFlashFilesets(request.Limit, ct).WaitAsync(ct);
        return new(new Result(result));
    }

    public sealed class Request(uint limit = 10)
    {
        [JsonPropertyName("limit")] public uint Limit { get; init; } = limit;
    }

    public sealed class Result(BotFlashFilesetResult result)
    {
        [JsonPropertyName("files")] public FlashFile[] Files { get; } = result.Entries.Select(x => new FlashFile(x)).ToArray();
    }

    public sealed class FlashFile(BotFlashFileEntry entry)
    {
        [JsonPropertyName("fileset_uuid")] public string FilesetUuid { get; } = entry.FilesetUuid;
        [JsonPropertyName("file_name")] public string FileName { get; } = entry.FileName;
        [JsonPropertyName("file_size")] public ulong FileSize { get; } = entry.FileSize;
        [JsonPropertyName("file_id")] public string? FileId { get; } = entry.FileId;
        [JsonPropertyName("download_url")] public string? DownloadUrl { get; } = entry.DownloadUrl;
    }
}
