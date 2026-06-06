namespace Lagrange.Core.Common.Entity;

public interface IBotFSEntry;

public class BotFileEntry(
    string fileId,
    string fileName,
    string parentDirectory,
    ulong fileSize,
    DateTime expireTime,
    DateTime modifiedTime,
    long uploaderUin,
    DateTime uploadedTime,
    uint downloadedTimes) : IBotFSEntry
{
    public string FileId { get; } = fileId;

    public string FileName { get; } = fileName;

    public string ParentDirectory { get; } = parentDirectory;

    public ulong FileSize { get; } = fileSize;

    public DateTime ExpireTime { get; } = expireTime;

    public DateTime ModifiedTime { get; } = modifiedTime;

    public long UploaderUin { get; } = uploaderUin;

    public DateTime UploadedTime { get; } = uploadedTime;

    public uint DownloadedTimes { get; } = downloadedTimes;
}

public class BotFolderEntry(
    string folderId,
    string parentFolderId,
    string folderName,
    DateTime createTime,
    DateTime modifiedTime,
    long creatorUin,
    uint totalFileCount) : IBotFSEntry
{
    public string FolderId { get; } = folderId;

    public string ParentFolderId { get; } = parentFolderId;

    public string FolderName { get; } = folderName;

    public DateTime CreateTime { get; } = createTime;

    public DateTime ModifiedTime { get; } = modifiedTime;

    public long CreatorUin { get; } = creatorUin;

    public uint TotalFileCount { get; } = totalFileCount;
}
