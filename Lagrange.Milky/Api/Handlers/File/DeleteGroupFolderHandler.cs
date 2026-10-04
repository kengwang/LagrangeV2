using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.File;

public sealed class DeleteGroupFolderHandler(BotContext lagrange) : Endpoint<DeleteGroupFolderHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/delete_group_folder");
    }
    private readonly BotContext _lagrange = lagrange;

    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
    {
        await _lagrange.GroupFSDeleteFolder(request.GroupId, request.FolderId, ct);
        return new MilkyApiResponse();
    }

    public sealed class Request(long groupId, string folderId)
    {
        [JsonPropertyName("group_id")] public required long GroupId { get; init; } = groupId;
        [JsonPropertyName("folder_id")] public required string FolderId { get; init; } = folderId;
    }
}
