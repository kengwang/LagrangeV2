using Lagrange.Core.Common.Response;
using Lagrange.Core.Events;

namespace Lagrange.Core.Internal.Events.System;

internal sealed class UploadQzoneImageEventReq(Stream image) : ProtocolEvent { public Stream Image { get; } = image; }
internal sealed class UploadQzoneImageEventResp(BotQzoneUploadResult result) : ProtocolEvent { public BotQzoneUploadResult Result { get; } = result; }
