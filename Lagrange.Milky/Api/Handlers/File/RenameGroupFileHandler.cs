using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.File;

[ApiHandler("rename_group_file")]
public sealed class RenameGroupFileHandler(BotContext lagrange) : INoResultApiHandler<RenameGroupFileHandler.Request>
{
    private readonly BotContext _lagrange = lagrange;

    public async ValueTask<MilkyApiResponse> HandleAsync(Request request, CancellationToken ct)
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
