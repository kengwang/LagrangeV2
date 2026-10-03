using Lagrange.Core.Common.Response;
using Lagrange.Core.Events;

namespace Lagrange.Core.Internal.Events.System;

internal sealed class UploadGroupAnnouncementImageEventReq(Stream image) : ProtocolEvent { public Stream Image { get; } = image; }
internal sealed class UploadGroupAnnouncementImageEventResp(BotGroupAnnouncementImage result) : ProtocolEvent { public BotGroupAnnouncementImage Result { get; } = result; }
