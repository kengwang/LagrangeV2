using Lagrange.Core.Common;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Services;
using Lagrange.Proto.Jce;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<ConfigPushEvent>(Protocols.Android)]
[Service("ConfigPushSvc.PushReq")]
internal sealed class ConfigPushService : BaseService<ConfigPushEvent, ConfigPushEvent>
{
    protected override ValueTask<ConfigPushEvent> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        try
        {
            return ValueTask.FromResult(new ConfigPushEvent(JceConfigPushParser.ParsePushReq(input.Span)));
        }
        catch (Exception e)
        {
            context.LogDebug(nameof(ConfigPushService), "Invalid ConfigPushSvc.PushReq payload: {0}", null, e.Message);
            return ValueTask.FromResult(new ConfigPushEvent(new ConfigPushPayload()));
        }
    }
}
