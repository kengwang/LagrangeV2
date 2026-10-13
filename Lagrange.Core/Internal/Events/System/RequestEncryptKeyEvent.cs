using Lagrange.Core.Common.Response;
using Lagrange.Core.Events;

namespace Lagrange.Core.Internal.Events.System;

internal sealed class RequestEncryptKeyEventReq : ProtocolEvent;

internal sealed class RequestEncryptKeyEventResp(BotDatabaseKeyResult result) : ProtocolEvent
{
    public BotDatabaseKeyResult Result { get; } = result;
}
