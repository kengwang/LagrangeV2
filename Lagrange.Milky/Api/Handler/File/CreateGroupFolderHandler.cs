using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handler.File;

[Api("create_group_folder")]
public class CreateGroupFolderHandler(BotContext bot) : IApiHandler<CreateGroupFolderParameter, CreateGroupFolderResult>
{
    private readonly BotContext _bot = bot;

    public async Task<CreateGroupFolderResult> HandleAsync(CreateGroupFolderParameter parameter, CancellationToken token)
    {
        return new CreateGroupFolderResult(await _bot.GroupFSCreateFolder(parameter.GroupId, parameter.FolderName));
    }
}

public class CreateGroupFolderParameter(long groupId, string folderName)
{
    [JsonRequired]
    [JsonPropertyName("group_id")]
    public long GroupId { get; init; } = groupId;

    [JsonRequired]
    [JsonPropertyName("folder_name")]
    public string FolderName { get; init; } = folderName;
}

public class CreateGroupFolderResult(string folderId)
{
    [JsonPropertyName("folder_id")]
    public string FolderId { get; } = folderId;
}
