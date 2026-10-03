namespace Lagrange.Core.Events.EventArgs;

public sealed class BotFriendFileUploadEvent(long peerUin, long userUin, string fileId, string fileName, long fileSize) : EventBase
{
    public long PeerUin { get; } = peerUin;
    public long UserUin { get; } = userUin;
    public string FileId { get; } = fileId;
    public string FileName { get; } = fileName;
    public long FileSize { get; } = fileSize;
    public override string ToEventMessage() => $"{nameof(BotFriendFileUploadEvent)}: {PeerUin}/{FileName}";
}
