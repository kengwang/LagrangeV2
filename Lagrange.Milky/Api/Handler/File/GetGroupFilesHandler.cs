using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Entity;
using Lagrange.Milky.Extension;

namespace Lagrange.Milky.Api.Handler.File;

[Api("get_group_files")]
public class GetGroupFilesHandler(BotContext bot) : IApiHandler<GetGroupFilesParameter, GetGroupFilesResult>
{
    private readonly BotContext _bot = bot;

    public async Task<GetGroupFilesResult> HandleAsync(GetGroupFilesParameter parameter, CancellationToken token)
    {
        var entries = await _bot.GroupFSList(parameter.GroupId, parameter.ParentFolderId);
        var files = new List<GroupFile>();
        var folders = new List<GroupFolder>();

        foreach (var entry in entries)
        {
            switch (entry)
            {
                case BotFileEntry file:
                    files.Add(new GroupFile(
                        parameter.GroupId,
                        file.FileId,
                        file.FileName,
                        file.ParentDirectory,
                        (long)file.FileSize,
                        file.UploadedTime.ToUnixTimeSeconds(),
                        file.ExpireTime == DateTime.UnixEpoch ? null : file.ExpireTime.ToUnixTimeSeconds(),
                        file.UploaderUin,
                        (int)file.DownloadedTimes
                    ));
                    break;
                case BotFolderEntry folder:
                    folders.Add(new GroupFolder(
                        parameter.GroupId,
                        folder.FolderId,
                        folder.ParentFolderId,
                        folder.FolderName,
                        folder.CreateTime.ToUnixTimeSeconds(),
                        folder.ModifiedTime.ToUnixTimeSeconds(),
                        folder.CreatorUin,
                        (int)folder.TotalFileCount
                    ));
                    break;
            }
        }

        return new GetGroupFilesResult(files, folders);
    }
}

public class GetGroupFilesParameter(long groupId, string parentFolderId = "/")
{
    [JsonRequired]
    [JsonPropertyName("group_id")]
    public long GroupId { get; init; } = groupId;

    [JsonPropertyName("parent_folder_id")]
    public string ParentFolderId { get; init; } = parentFolderId;
}

public class GetGroupFilesResult(IEnumerable<GroupFile> files, IEnumerable<GroupFolder> folders)
{
    [JsonPropertyName("files")]
    public IEnumerable<GroupFile> Files { get; } = files;

    [JsonPropertyName("folders")]
    public IEnumerable<GroupFolder> Folders { get; } = folders;
}
