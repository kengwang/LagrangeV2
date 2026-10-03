using Lagrange.Core.Common.Response;
using Lagrange.Core.Events;

namespace Lagrange.Core.Internal.Events.System;

internal sealed class PublishGroupAnnouncementEventReq(long groupUin, string content, BotGroupAnnouncementOptions options) : ProtocolEvent { public long GroupUin { get; } = groupUin; public string Content { get; } = content; public BotGroupAnnouncementOptions Options { get; } = options; }
internal sealed class PublishGroupAnnouncementEventResp : ProtocolEvent { public static PublishGroupAnnouncementEventResp Instance { get; } = new(); }
