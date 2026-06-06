using Lagrange.Core.Common.Entity;

namespace Lagrange.Core.Internal.Events.Message;

internal class GroupFSListEventReq(long groupUin, string parentDirectory, uint startIndex, uint fileCount) : ProtocolEvent
{
    public long GroupUin { get; } = groupUin;

    public string ParentDirectory { get; } = parentDirectory;

    public uint StartIndex { get; } = startIndex;

    public uint FileCount { get; } = fileCount;
}

internal class GroupFSListEventResp(IReadOnlyList<IBotFSEntry> entries, bool isEnd) : ProtocolEvent
{
    public IReadOnlyList<IBotFSEntry> Entries { get; } = entries;

    public bool IsEnd { get; } = isEnd;
}

internal class GroupFSCreateFolderEventReq(long groupUin, string folderName) : ProtocolEvent
{
    public long GroupUin { get; } = groupUin;

    public string FolderName { get; } = folderName;
}

internal class GroupFSCreateFolderEventResp(string folderId) : ProtocolEvent
{
    public string FolderId { get; } = folderId;
}

internal class GroupFSRenameFolderEventReq(long groupUin, string folderId, string newFolderName) : ProtocolEvent
{
    public long GroupUin { get; } = groupUin;

    public string FolderId { get; } = folderId;

    public string NewFolderName { get; } = newFolderName;
}

internal class GroupFSRenameFolderEventResp : ProtocolEvent;

internal class GroupFSDeleteFolderEventReq(long groupUin, string folderId) : ProtocolEvent
{
    public long GroupUin { get; } = groupUin;

    public string FolderId { get; } = folderId;
}

internal class GroupFSDeleteFolderEventResp : ProtocolEvent;
