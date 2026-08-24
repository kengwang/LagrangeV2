using Lagrange.Core.Common;
using Lagrange.Core.Events;
using Lagrange.Core.Internal.Context;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Logic;

[EventSubscribe<ConfigPushEvent>(Protocols.Android)]
internal sealed class ConfigPushLogic(BotContext context) : ILogic
{
    public ValueTask Incoming(ProtocolEvent e)
    {
        if (e is not ConfigPushEvent push || push.Payload.Type == 0) return ValueTask.CompletedTask;

        if (push.Payload.Type == 1)
        {
            context.SocketContext.SetPushedServers(push.Payload.Servers);
            context.LogDebug(nameof(ConfigPushLogic), "Received {0} pushed SSO server addresses.", null, push.Payload.Servers.Length);
        }
        else if (push.Payload.Type == 2)
        {
            if (push.Payload.FileStorage is { } storage) context.HighwayContext.ApplyPush(storage);
            context.LogDebug(nameof(ConfigPushLogic), "Received pushed file storage session information.");
        }
        else
            context.LogDebug(nameof(ConfigPushLogic), "Received unknown ConfigPushSvc.PushReq type: {0}", null, push.Payload.Type);

        return ValueTask.CompletedTask;
    }
}
