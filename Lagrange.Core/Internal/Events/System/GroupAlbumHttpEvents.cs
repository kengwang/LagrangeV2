using Lagrange.Core.Common.Response;
using Lagrange.Core.Events;

namespace Lagrange.Core.Internal.Events.System;

internal sealed class GetGroupAlbumsEventReq(long groupUin, string attachInfo) : ProtocolEvent { public long GroupUin { get; } = groupUin; public string AttachInfo { get; } = attachInfo; }
internal sealed class GetGroupAlbumsEventResp(BotGroupAlbumResult result) : ProtocolEvent { public BotGroupAlbumResult Result { get; } = result; }
