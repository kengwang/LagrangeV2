using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.File;

public sealed class CommitFlashFileHandler(BotContext lagrange) : Endpoint<CommitFlashFileHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/commit_flash_file");
    }
    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
    {
        await lagrange.CommitFlashFile(request.FilesetUuid, request.UploadKey, request.FileUuid, request.FileName, request.FileSize, request.Index, request.FormatCode, ct).WaitAsync(ct);
        return new MilkyApiResponse();
    }
    public sealed class Request(string filesetUuid, string uploadKey, string fileUuid, string fileName, ulong fileSize)
    {
        [JsonPropertyName("fileset_uuid")] public required string FilesetUuid { get; init; } = filesetUuid;
        [JsonPropertyName("upload_key")] public required string UploadKey { get; init; } = uploadKey;
        [JsonPropertyName("file_uuid")] public required string FileUuid { get; init; } = fileUuid;
        [JsonPropertyName("file_name")] public required string FileName { get; init; } = fileName;
        [JsonPropertyName("file_size")] public required ulong FileSize { get; init; } = fileSize;
        [JsonPropertyName("index")] public uint Index { get; init; } = 1;
        [JsonPropertyName("format_code")] public uint FormatCode { get; init; } = 26;
    }
}
