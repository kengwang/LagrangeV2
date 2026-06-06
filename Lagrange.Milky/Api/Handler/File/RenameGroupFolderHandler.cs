using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handler.File;

[Api("rename_group_folder")]
public class RenameGroupFolderHandler(BotContext bot) : IEmptyResultApiHandler<RenameGroupFolderParameter>
{
    private readonly BotContext _bot = bot;

    public Task HandleAsync(RenameGroupFolderParameter parameter, CancellationToken token)
    {
        return _bot.GroupFSRenameFolder(parameter.GroupId, parameter.FolderId, parameter.NewFolderName);
    }
}

public class RenameGroupFolderParameter(long groupId, string folderId, string newFolderName)
{
    [JsonRequired]
    [JsonPropertyName("group_id")]
    public long GroupId { get; init; } = groupId;

    [JsonRequired]
    [JsonPropertyName("folder_id")]
    public string FolderId { get; init; } = folderId;

    [JsonRequired]
    [JsonPropertyName("new_folder_name")]
    public string NewFolderName { get; init; } = newFolderName;
}
