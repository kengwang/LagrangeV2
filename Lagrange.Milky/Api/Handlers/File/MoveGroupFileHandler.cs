using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.File;

public sealed class MoveGroupFileHandler(BotContext lagrange) : Endpoint<MoveGroupFileHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/move_group_file");
    }
    private readonly BotContext _lagrange = lagrange;

    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
    {
        await _lagrange.GroupFSMove(
            request.GroupId,
            request.FileId,
            request.ParentFolderId,
            request.TargetFolderId,
            ct
        );
        return new MilkyApiResponse();
    }

    public sealed class Request(long groupId, string fileId, string parentFolderId = "/", string targetFolderId = "/")
    {
        [JsonPropertyName("group_id")] public required long GroupId { get; init; } = groupId;
        [JsonPropertyName("file_id")] public required string FileId { get; init; } = fileId;
        [JsonPropertyName("parent_folder_id")] public string ParentFolderId { get; init; } = parentFolderId;
        [JsonPropertyName("target_folder_id")] public string TargetFolderId { get; init; } = targetFolderId;
    }
}
