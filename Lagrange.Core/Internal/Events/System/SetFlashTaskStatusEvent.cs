using Lagrange.Core.Events;

namespace Lagrange.Core.Internal.Events.System;

internal sealed class SetFlashTaskStatusEventReq(string filesetUuid, uint status) : ProtocolEvent { public string FilesetUuid { get; } = filesetUuid; public uint Status { get; } = status; }
internal sealed class SetFlashTaskStatusEventResp : ProtocolEvent;
