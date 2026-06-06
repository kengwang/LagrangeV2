using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity;

public class GroupFile(long groupId, string fileId, string fileName, string parentFolderId, long fileSize, long uploadedTime, long? expireTime, long uploaderId, int downloadedTimes)
{
    [JsonPropertyName("group_id")]
    public long GroupId { get; } = groupId;

    [JsonPropertyName("file_id")]
    public string FileId { get; } = fileId;

    [JsonPropertyName("file_name")]
    public string FileName { get; } = fileName;

    [JsonPropertyName("parent_folder_id")]
    public string ParentFolderId { get; } = parentFolderId;

    [JsonPropertyName("file_size")]
    public long FileSize { get; } = fileSize;

    [JsonPropertyName("uploaded_time")]
    public long UploadedTime { get; } = uploadedTime;

    [JsonPropertyName("expire_time")]
    public long? ExpireTime { get; } = expireTime;

    [JsonPropertyName("uploader_id")]
    public long UploaderId { get; } = uploaderId;

    [JsonPropertyName("downloaded_times")]
    public int DownloadedTimes { get; } = downloadedTimes;
}

public class GroupFolder(long groupId, string folderId, string parentFolderId, string folderName, long createdTime, long lastModifiedTime, long creatorId, int fileCount)
{
    [JsonPropertyName("group_id")]
    public long GroupId { get; } = groupId;

    [JsonPropertyName("folder_id")]
    public string FolderId { get; } = folderId;

    [JsonPropertyName("parent_folder_id")]
    public string ParentFolderId { get; } = parentFolderId;

    [JsonPropertyName("folder_name")]
    public string FolderName { get; } = folderName;

    [JsonPropertyName("created_time")]
    public long CreatedTime { get; } = createdTime;

    [JsonPropertyName("last_modified_time")]
    public long LastModifiedTime { get; } = lastModifiedTime;

    [JsonPropertyName("creator_id")]
    public long CreatorId { get; } = creatorId;

    [JsonPropertyName("file_count")]
    public int FileCount { get; } = fileCount;
}
