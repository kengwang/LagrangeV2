using Lagrange.Core.Events;

namespace Lagrange.Core.Internal.Events.System;

internal class SetInputStatusEventReq(long userUin, uint eventType) : ProtocolEvent
{
    public long UserUin { get; } = userUin;
    public uint EventType { get; } = eventType;
}

internal class SetInputStatusEventResp : ProtocolEvent
{
    public static readonly SetInputStatusEventResp Default = new();
}
