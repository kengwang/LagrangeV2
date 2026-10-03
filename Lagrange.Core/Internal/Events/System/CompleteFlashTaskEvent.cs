using Lagrange.Core.Events;

namespace Lagrange.Core.Internal.Events.System;

internal sealed class CompleteFlashTaskEventReq(string filesetUuid) : ProtocolEvent { public string FilesetUuid { get; } = filesetUuid; }
internal sealed class CompleteFlashTaskEventResp : ProtocolEvent;
