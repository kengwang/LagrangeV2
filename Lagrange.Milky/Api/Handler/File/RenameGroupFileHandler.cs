using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handler.File;

[Api("rename_group_file")]
public class RenameGroupFileHandler(BotContext bot) : IEmptyResultApiHandler<RenameGroupFileParameter>
{
    private readonly BotContext _bot = bot;

    public Task HandleAsync(RenameGroupFileParameter parameter, CancellationToken token)
    {
        return _bot.GroupFSRename(parameter.GroupId, parameter.FileId, parameter.ParentFolderId, parameter.NewFileName);
    }
}

public class RenameGroupFileParameter(long groupId, string fileId, string newFileName, string parentFolderId = "/")
{
    [JsonRequired]
    [JsonPropertyName("group_id")]
    public long GroupId { get; init; } = groupId;

    [JsonRequired]
    [JsonPropertyName("file_id")]
    public string FileId { get; init; } = fileId;

    [JsonPropertyName("parent_folder_id")]
    public string ParentFolderId { get; init; } = parentFolderId;

    [JsonRequired]
    [JsonPropertyName("new_file_name")]
    public string NewFileName { get; init; } = newFileName;
}
