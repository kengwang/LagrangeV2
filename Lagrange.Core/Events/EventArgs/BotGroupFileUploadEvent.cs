namespace Lagrange.Core.Events.EventArgs;

public sealed class BotGroupFileUploadEvent(long groupUin, long userUin, string fileId, string fileName, long fileSize, long busId = 0) : EventBase
{
    public long GroupUin { get; } = groupUin;
    public long UserUin { get; } = userUin;
    public string FileId { get; } = fileId;
    public string FileName { get; } = fileName;
    public long FileSize { get; } = fileSize;
    public long BusId { get; } = busId;
    public override string ToEventMessage() => $"{nameof(BotGroupFileUploadEvent)}: {GroupUin}/{FileName}";
}
