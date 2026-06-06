using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handler.File;

[Api("move_group_file")]
public class MoveGroupFileHandler(BotContext bot) : IEmptyResultApiHandler<MoveGroupFileParameter>
{
    private readonly BotContext _bot = bot;

    public Task HandleAsync(MoveGroupFileParameter parameter, CancellationToken token)
    {
        return _bot.GroupFSMove(parameter.GroupId, parameter.FileId, parameter.ParentFolderId, parameter.TargetFolderId);
    }
}

public class MoveGroupFileParameter(long groupId, string fileId, string parentFolderId = "/", string targetFolderId = "/")
{
    [JsonRequired]
    [JsonPropertyName("group_id")]
    public long GroupId { get; init; } = groupId;

    [JsonRequired]
    [JsonPropertyName("file_id")]
    public string FileId { get; init; } = fileId;

    [JsonPropertyName("parent_folder_id")]
    public string ParentFolderId { get; init; } = parentFolderId;

    [JsonPropertyName("target_folder_id")]
    public string TargetFolderId { get; init; } = targetFolderId;
}
