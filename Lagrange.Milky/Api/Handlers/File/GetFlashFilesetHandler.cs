using FastEndpoints;
using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.File;

public sealed class GetFlashFilesetHandler(BotContext lagrange) : Endpoint<GetFlashFilesetHandler.Request, MilkyApiResponse<GetFlashFilesetHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/get_flash_fileset");
    }
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        var result = await lagrange.GetFlashFileset(request.FilesetUuid, ct).WaitAsync(ct);
        return new(new Result(result));
    }

    public sealed class Request(string filesetUuid)
    {
        [JsonPropertyName("fileset_uuid")] public required string FilesetUuid { get; init; } = filesetUuid;
    }

    public sealed class Result(BotFlashFilesetResult result)
    {
        [JsonPropertyName("files")] public FlashFile[] Files { get; } = result.Entries.Select(x => new FlashFile(x)).ToArray();
    }

    public sealed class FlashFile(BotFlashFileEntry entry)
    {
        [JsonPropertyName("fileset_uuid")] public string FilesetUuid { get; } = entry.FilesetUuid;
        [JsonPropertyName("file_name")] public string FileName { get; } = entry.FileName;
        [JsonPropertyName("original_name")] public string OriginalName { get; } = entry.OriginalName;
        [JsonPropertyName("file_type")] public uint FileType { get; } = entry.FileType;
        [JsonPropertyName("file_size")] public ulong FileSize { get; } = entry.FileSize;
        [JsonPropertyName("upload_url")] public string? UploadUrl { get; } = entry.UploadUrl;
        [JsonPropertyName("file_id")] public string? FileId { get; } = entry.FileId;
        [JsonPropertyName("download_url")] public string? DownloadUrl { get; } = entry.DownloadUrl;
    }
}
