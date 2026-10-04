using Lagrange.Core.Events;
using Lagrange.Core.Internal.Packets.Service;

namespace Lagrange.Core.Internal.Events.System;

internal sealed class CommitFlashFileEventReq(string filesetUuid, string uploadKey, string fileUuid, string fileName, ulong fileSize, uint index, uint formatCode) : ProtocolEvent
{
    public List<D93D0Info>? Entries { get; init; }
    public string FilesetUuid { get; } = filesetUuid; public string UploadKey { get; } = uploadKey; public string FileUuid { get; } = fileUuid; public string FileName { get; } = fileName; public ulong FileSize { get; } = fileSize; public uint Index { get; } = index; public uint FormatCode { get; } = formatCode;
}
internal sealed class CommitFlashFileEventResp : ProtocolEvent;
