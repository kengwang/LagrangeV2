using Lagrange.Core.Common.Response;
using Lagrange.Core.Events;

namespace Lagrange.Core.Internal.Events.System;

internal sealed class UploadGroupAlbumVideoEventReq(long groupUin, string albumId, string albumName, Stream video, Stream cover,
    long durationMilliseconds, string fileName) : ProtocolEvent
{
    public long GroupUin { get; } = groupUin;
    public string AlbumId { get; } = albumId;
    public string AlbumName { get; } = albumName;
    public Stream Video { get; } = video;
    public Stream Cover { get; } = cover;
    public long DurationMilliseconds { get; } = durationMilliseconds;
    public string FileName { get; } = fileName;
}

internal sealed class UploadGroupAlbumVideoEventResp(BotGroupAlbumVideoUploadResult result) : ProtocolEvent
{
    public BotGroupAlbumVideoUploadResult Result { get; } = result;
}
