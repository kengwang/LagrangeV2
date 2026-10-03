using Lagrange.Core.Common.Response;
using Lagrange.Core.Events;

namespace Lagrange.Core.Internal.Events.System;

internal sealed class CreateFlashTaskEventReq(string fileName, ulong fileSize, uint fileType) : ProtocolEvent
{
    public string FileName { get; } = fileName;
    public ulong FileSize { get; } = fileSize;
    public uint FileType { get; } = fileType;
}
internal sealed class CreateFlashTaskEventResp(BotFlashCreateResult result) : ProtocolEvent { public BotFlashCreateResult Result { get; } = result; }
