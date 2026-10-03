using Lagrange.Core.Common.Response;
using Lagrange.Core.Events;

namespace Lagrange.Core.Internal.Events.System;

internal sealed class GetGroupAlbumMediaEventReq(long groupUin, string albumId, string cursor) : ProtocolEvent { public long GroupUin { get; } = groupUin; public string AlbumId { get; } = albumId; public string Cursor { get; } = cursor; }
internal sealed class GetGroupAlbumMediaEventResp(BotGroupAlbumMediaResult result) : ProtocolEvent { public BotGroupAlbumMediaResult Result { get; } = result; }
internal sealed class SetGroupAlbumLikeEventReq(long groupUin, string albumId, string batchId, string? mediaId, bool isLike) : ProtocolEvent { public long GroupUin { get; } = groupUin; public string AlbumId { get; } = albumId; public string BatchId { get; } = batchId; public string? MediaId { get; } = mediaId; public bool IsLike { get; } = isLike; }
internal sealed class SetGroupAlbumLikeEventResp : ProtocolEvent { public static SetGroupAlbumLikeEventResp Instance { get; } = new(); }
internal sealed class DeleteGroupAlbumMediaEventReq(long groupUin, string albumId, string mediaId, string? batchId) : ProtocolEvent { public long GroupUin { get; } = groupUin; public string AlbumId { get; } = albumId; public string MediaId { get; } = mediaId; public string? BatchId { get; } = batchId; }
internal sealed class DeleteGroupAlbumMediaEventResp : ProtocolEvent { public static DeleteGroupAlbumMediaEventResp Instance { get; } = new(); }
internal sealed class CommentGroupAlbumMediaEventReq(long groupUin, string albumId, string mediaId, string content) : ProtocolEvent { public long GroupUin { get; } = groupUin; public string AlbumId { get; } = albumId; public string MediaId { get; } = mediaId; public string Content { get; } = content; }
internal sealed class CommentGroupAlbumMediaEventResp(string commentId) : ProtocolEvent { public string CommentId { get; } = commentId; }
