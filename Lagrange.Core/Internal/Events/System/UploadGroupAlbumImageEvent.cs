using Lagrange.Core.Common.Response;
using Lagrange.Core.Events;

namespace Lagrange.Core.Internal.Events.System;

internal sealed class UploadGroupAlbumImageEventReq(long groupUin, string albumId, string albumName, Stream image, string fileName) : ProtocolEvent
{
    public long GroupUin { get; } = groupUin;
    public string AlbumId { get; } = albumId;
    public string AlbumName { get; } = albumName;
    public Stream Image { get; } = image;
    public string FileName { get; } = fileName;
}

internal sealed class UploadGroupAlbumImageEventResp(BotGroupAlbumUploadResult result) : ProtocolEvent
{
    public BotGroupAlbumUploadResult Result { get; } = result;
}
