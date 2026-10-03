using Lagrange.Core.Events;

namespace Lagrange.Core.Internal.Events.System;

internal sealed class AddCustomFaceEventReq(byte[] md5, uint fileSize) : ProtocolEvent
{
    public byte[] Md5 { get; } = md5;
    public uint FileSize { get; } = fileSize;
}
internal sealed class AddCustomFaceEventResp(byte[] token) : ProtocolEvent
{
    public byte[] Token { get; } = token;
}
