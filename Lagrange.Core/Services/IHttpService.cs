using Lagrange.Core.Events;

namespace Lagrange.Core.Services;

internal interface IHttpService
{
    Type RequestType { get; }
    ValueTask<ProtocolEvent> ExecuteAsync(BotContext context, ProtocolEvent request, CancellationToken cancellationToken);
}
