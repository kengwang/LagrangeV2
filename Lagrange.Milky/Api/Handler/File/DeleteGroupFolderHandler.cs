using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handler.File;

[Api("delete_group_folder")]
public class DeleteGroupFolderHandler(BotContext bot) : IEmptyResultApiHandler<DeleteGroupFolderParameter>
{
    private readonly BotContext _bot = bot;

    public Task HandleAsync(DeleteGroupFolderParameter parameter, CancellationToken token)
    {
        return _bot.GroupFSDeleteFolder(parameter.GroupId, parameter.FolderId);
    }
}

public class DeleteGroupFolderParameter(long groupId, string folderId)
{
    [JsonRequired]
    [JsonPropertyName("group_id")]
    public long GroupId { get; init; } = groupId;

    [JsonRequired]
    [JsonPropertyName("folder_id")]
    public string FolderId { get; init; } = folderId;
}
