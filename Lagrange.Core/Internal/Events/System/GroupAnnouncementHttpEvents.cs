using Lagrange.Core.Common.Response;
using Lagrange.Core.Events;

namespace Lagrange.Core.Internal.Events.System;

internal sealed class GetGroupAnnouncementsEventReq(long groupUin, int start, int count) : ProtocolEvent { public long GroupUin { get; } = groupUin; public int Start { get; } = start; public int Count { get; } = count; }
internal sealed class GetGroupAnnouncementsEventResp(BotGroupAnnouncementResult result) : ProtocolEvent { public BotGroupAnnouncementResult Result { get; } = result; }
internal sealed class DeleteGroupAnnouncementEventReq(long groupUin, string announcementId) : ProtocolEvent { public long GroupUin { get; } = groupUin; public string AnnouncementId { get; } = announcementId; }
internal sealed class DeleteGroupAnnouncementEventResp : ProtocolEvent { public static DeleteGroupAnnouncementEventResp Instance { get; } = new(); }
