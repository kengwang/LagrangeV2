using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Converters;

namespace Lagrange.Milky.Api.Handlers.File;

public sealed class UploadGroupFileHandler(BotContext lagrange, ResourceConverter resourceConverter) : Endpoint<UploadGroupFileHandler.Request, MilkyApiResponse<UploadGroupFileHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/upload_group_file");
    }
    private readonly BotContext _lagrange = lagrange;
    private readonly ResourceConverter _resourceConverter = resourceConverter;

    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        using var stream = await _resourceConverter.UriToStreamAsync(request.FileUri, ct);
        string fileId = await _lagrange.SendGroupFile(
            request.GroupId,
            stream,
            request.FileName,
            request.ParentFolderId,
            ct
        ).WaitAsync(ct);
        return new MilkyApiResponse<Result>(new Result
        {
            FileId = fileId
        });
    }

    public sealed class Request(long groupId, string fileUri, string fileName, string parentFolderId = "/")
    {
        [JsonPropertyName("group_id")] public required long GroupId { get; init; } = groupId;
        [JsonPropertyName("file_uri")] public required string FileUri { get; init; } = fileUri;
        [JsonPropertyName("file_name")] public required string FileName { get; init; } = fileName;
        [JsonPropertyName("parent_folder_id")] public string ParentFolderId { get; init; } = parentFolderId;
    }

    public sealed class Result
    {
        [JsonPropertyName("file_id")] public required string FileId { get; init; }
    }
}
