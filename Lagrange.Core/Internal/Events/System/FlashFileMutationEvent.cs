using Lagrange.Core.Events;

namespace Lagrange.Core.Internal.Events.System;

internal sealed class DeleteFlashFileEventReq(string filesetUuid) : ProtocolEvent { public string FilesetUuid { get; } = filesetUuid; }
internal sealed class DeleteFlashFileEventResp : ProtocolEvent;
internal sealed class RenameFlashFileEventReq(string filesetUuid, string newName) : ProtocolEvent { public string FilesetUuid { get; } = filesetUuid; public string NewName { get; } = newName; }
internal sealed class RenameFlashFileEventResp : ProtocolEvent;
internal sealed class SendFlashMessageEventReq(long? userUin, long? groupUin, string filesetUuid) : ProtocolEvent { public long? UserUin { get; } = userUin; public long? GroupUin { get; } = groupUin; public string FilesetUuid { get; } = filesetUuid; }
internal sealed class SendFlashMessageEventResp : ProtocolEvent;
