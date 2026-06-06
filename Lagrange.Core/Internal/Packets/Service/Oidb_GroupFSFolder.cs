using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class GroupFSListRequest
{
    [ProtoMember(2)] public GroupFSListRequestBody List { get; set; }
}

[ProtoPackable]
internal partial class GroupFSListRequestBody
{
    [ProtoMember(1)] public long GroupUin { get; set; }

    [ProtoMember(2)] public uint AppId { get; set; }

    [ProtoMember(3)] public string TargetDirectory { get; set; }

    [ProtoMember(5)] public uint FileCount { get; set; }

    [ProtoMember(9)] public uint SortBy { get; set; }

    [ProtoMember(13)] public uint StartIndex { get; set; }

    [ProtoMember(17)] public uint Field17 { get; set; }

    [ProtoMember(18)] public uint Field18 { get; set; }
}

[ProtoPackable]
internal partial class GroupFSListResponse
{
    [ProtoMember(2)] public GroupFSListResponseBody? List { get; set; }
}

[ProtoPackable]
internal partial class GroupFSListResponseBody
{
    [ProtoMember(1)] public int RetCode { get; set; }

    [ProtoMember(2)] public string RetMsg { get; set; }

    [ProtoMember(4)] public bool IsEnd { get; set; }

    [ProtoMember(5)] public List<GroupFSListResponseItem>? Items { get; set; }
}

[ProtoPackable]
internal partial class GroupFSListResponseItem
{
    [ProtoMember(1)] public uint Type { get; set; }

    [ProtoMember(2)] public GroupFSListFolderInfo FolderInfo { get; set; }

    [ProtoMember(3)] public GroupFSListFileInfo FileInfo { get; set; }
}

[ProtoPackable]
internal partial class GroupFSListFolderInfo
{
    [ProtoMember(1)] public string FolderId { get; set; }

    [ProtoMember(2)] public string ParentDirectoryId { get; set; }

    [ProtoMember(3)] public string FolderName { get; set; }

    [ProtoMember(4)] public uint CreateTime { get; set; }

    [ProtoMember(5)] public uint ModifiedTime { get; set; }

    [ProtoMember(6)] public long CreatorUin { get; set; }

    [ProtoMember(8)] public uint TotalFileCount { get; set; }
}

[ProtoPackable]
internal partial class GroupFSListFileInfo
{
    [ProtoMember(1)] public string FileId { get; set; }

    [ProtoMember(2)] public string FileName { get; set; }

    [ProtoMember(3)] public ulong FileSize { get; set; }

    [ProtoMember(6)] public uint UploadedTime { get; set; }

    [ProtoMember(7)] public uint ExpireTime { get; set; }

    [ProtoMember(8)] public uint ModifiedTime { get; set; }

    [ProtoMember(9)] public uint DownloadedTimes { get; set; }

    [ProtoMember(15)] public long UploaderUin { get; set; }

    [ProtoMember(16)] public string ParentDirectory { get; set; }
}

[ProtoPackable]
internal partial class GroupFSFolderRequest
{
    [ProtoMember(1)] public GroupFSCreateFolderRequestBody? Create { get; set; }

    [ProtoMember(2)] public GroupFSDeleteFolderRequestBody? Delete { get; set; }

    [ProtoMember(3)] public GroupFSRenameFolderRequestBody? Rename { get; set; }
}

[ProtoPackable]
internal partial class GroupFSCreateFolderRequestBody
{
    [ProtoMember(1)] public long GroupUin { get; set; }

    [ProtoMember(3)] public string RootDirectory { get; set; }

    [ProtoMember(4)] public string FolderName { get; set; }
}

[ProtoPackable]
internal partial class GroupFSDeleteFolderRequestBody
{
    [ProtoMember(1)] public long GroupUin { get; set; }

    [ProtoMember(3)] public string FolderId { get; set; }
}

[ProtoPackable]
internal partial class GroupFSRenameFolderRequestBody
{
    [ProtoMember(1)] public long GroupUin { get; set; }

    [ProtoMember(3)] public string FolderId { get; set; }

    [ProtoMember(4)] public string NewFolderName { get; set; }
}

[ProtoPackable]
internal partial class GroupFSFolderResponse
{
    [ProtoMember(1)] public GroupFSCreateFolderResponseBody? Create { get; set; }

    [ProtoMember(2)] public GroupFSFolderOperateResponseBody? Delete { get; set; }

    [ProtoMember(3)] public GroupFSFolderOperateResponseBody? Rename { get; set; }
}

[ProtoPackable]
internal partial class GroupFSCreateFolderResponseBody
{
    [ProtoMember(1)] public int RetCode { get; set; }

    [ProtoMember(2)] public string RetMsg { get; set; }

    [ProtoMember(4)] public GroupFSCreateFolderInfo FolderInfo { get; set; }
}

[ProtoPackable]
internal partial class GroupFSCreateFolderInfo
{
    [ProtoMember(1)] public string FolderId { get; set; }
}

[ProtoPackable]
internal partial class GroupFSFolderOperateResponseBody
{
    [ProtoMember(1)] public int RetCode { get; set; }

    [ProtoMember(2)] public string RetMsg { get; set; }
}
