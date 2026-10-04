using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.File;

public sealed class CreateGroupFolderHandler(BotContext lagrange) : Endpoint<CreateGroupFolderHandler.Request, MilkyApiResponse<CreateGroupFolderHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/create_group_folder");
    }
    private readonly BotContext _lagrange = lagrange;

    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        string folderId = await _lagrange.GroupFSCreateFolder(request.GroupId, request.FolderName, cancellationToken: ct);
        return new MilkyApiResponse<Result>(new Result
        {
            FolderId = folderId
        });
    }

    public sealed class Request(long groupId, string folderName)
    {
        [JsonPropertyName("group_id")] public required long GroupId { get; init; } = groupId;
        [JsonPropertyName("folder_name")] public required string FolderName { get; init; } = folderName;
    }

    public sealed class Result
    {
        [JsonPropertyName("folder_id")] public required string FolderId { get; init; }
    }
}
