using Lagrange.Core.Events;
using Lagrange.Proto.Jce;

namespace Lagrange.Core.Internal.Events.System;

internal sealed class ConfigPushEvent(ConfigPushPayload payload) : ProtocolEvent
{
    public ConfigPushPayload Payload { get; } = payload;
}
