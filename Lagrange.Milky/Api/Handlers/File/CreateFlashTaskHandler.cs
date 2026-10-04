using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Common.Response;

namespace Lagrange.Milky.Api.Handlers.File;

public sealed class CreateFlashTaskHandler(BotContext lagrange) : Endpoint<CreateFlashTaskHandler.Request, MilkyApiResponse<CreateFlashTaskHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/create_flash_task");
    }
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        var result = await lagrange.CreateFlashTask(request.FileName, request.FileSize, request.FileType, ct).WaitAsync(ct);
        return new(new Result(result));
    }
    public sealed class Request(string fileName, ulong fileSize, uint fileType = 7)
    {
        [JsonPropertyName("file_name")] public required string FileName { get; init; } = fileName;
        [JsonPropertyName("file_size")] public required ulong FileSize { get; init; } = fileSize;
        [JsonPropertyName("file_type")] public uint FileType { get; init; } = fileType;
    }
    public sealed class Result(BotFlashCreateResult result)
    {
        [JsonPropertyName("fileset_uuid")] public string FilesetUuid { get; } = result.FilesetUuid;
        [JsonPropertyName("upload_key")] public string UploadKey { get; } = result.UploadKey;
        [JsonPropertyName("upload_url")] public string? UploadUrl { get; } = result.UploadUrl;
        [JsonPropertyName("expire")] public ulong Expire { get; } = result.Expire;
        [JsonPropertyName("ttl")] public uint Ttl { get; } = result.Ttl;
    }
}
