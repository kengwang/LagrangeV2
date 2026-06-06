namespace Lagrange.Core.Internal.Events.Message;

internal class PrivateFSDownloadEventReq(string fileId, string fileHash, string receiverUid) : ProtocolEvent
{
    public string FileId { get; } = fileId;

    public string FileHash { get; } = fileHash;

    public string ReceiverUid { get; } = receiverUid;
}

internal class PrivateFSDownloadEventResp(string downloadUrl) : ProtocolEvent
{
    public string DownloadUrl { get; } = downloadUrl;
}
