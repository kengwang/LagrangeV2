using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.File;

public sealed class RenameGroupFileHandler(BotContext lagrange) : Endpoint<RenameGroupFileHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/rename_group_file");
    }
    private readonly BotContext _lagrange = lagrange;

    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
    {
        await _lagrange.RenameGroupFile(request.GroupId, request.FileId, request.ParentDirectory, request.NewFileName, ct);
        return new MilkyApiResponse();
    }

    public sealed class Request(long groupId, string fileId, string parentDirectory, string newFileName)
    {
        [JsonPropertyName("group_id")] public required long GroupId { get; init; } = groupId;
        [JsonPropertyName("file_id")] public required string FileId { get; init; } = fileId;
        [JsonPropertyName("parent_directory")] public required string ParentDirectory { get; init; } = parentDirectory;
        [JsonPropertyName("new_file_name")] public required string NewFileName { get; init; } = newFileName;
    }
}
