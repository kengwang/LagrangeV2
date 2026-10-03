using Lagrange.Core.Events;

namespace Lagrange.Core.Services;

internal interface IHttpService
{
    Type RequestType { get; }
    void Configure(HttpServiceAttribute metadata);
    ValueTask<ProtocolEvent> ExecuteAsync(BotContext context, ProtocolEvent request, CancellationToken cancellationToken);
}
