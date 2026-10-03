using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.File;

[ApiHandler("commit_flash_file")]
public sealed class CommitFlashFileHandler(BotContext lagrange) : INoResultApiHandler<CommitFlashFileHandler.Request>
{
    public async ValueTask<MilkyApiResponse> HandleAsync(Request request, CancellationToken ct)
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
