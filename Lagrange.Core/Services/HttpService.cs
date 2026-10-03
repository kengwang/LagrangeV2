using Lagrange.Core.Internal.Context;
using Lagrange.Core.Events;

namespace Lagrange.Core.Services;

/// <summary>Base class for QQ web services that use the BotContext HTTP session.</summary>
public abstract class HttpService<TRequest, TResponse> : IHttpService
    where TRequest : ProtocolEvent
    where TResponse : ProtocolEvent
{
    private readonly IReadOnlyList<string> _cookieDomains;

    protected HttpService(params string[] cookieDomains)
    {
        _cookieDomains = cookieDomains ?? throw new ArgumentNullException(nameof(cookieDomains));
    }

    public virtual async Task<TResponse> ExecuteAsync(BotContext context, TRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        var session = context.HttpSessionContext;
        using var httpRequest = await BuildRequestAsync(context, request, cancellationToken);
        await session.PrepareRequestAsync(httpRequest, _cookieDomains, cancellationToken);
        using var response = await session.SendAsync(httpRequest, cancellationToken);
        var payload = await session.ReadResponseAsync(response, cancellationToken);
        return await ParseResponseAsync(context, request, response, payload, cancellationToken);
    }

    protected async Task<ReadOnlyMemory<byte>> SendRequestAsync(BotContext context, HttpRequestMessage message, CancellationToken cancellationToken)
    {
        await context.HttpSessionContext.PrepareRequestAsync(message, _cookieDomains, cancellationToken);
        using var response = await context.HttpSessionContext.SendAsync(message, cancellationToken);
        return await context.HttpSessionContext.ReadResponseAsync(response, cancellationToken);
    }

    Type IHttpService.RequestType => typeof(TRequest);

    async ValueTask<ProtocolEvent> IHttpService.ExecuteAsync(BotContext context, ProtocolEvent request, CancellationToken cancellationToken) =>
        await ExecuteAsync(context, (TRequest)request, cancellationToken);

    protected abstract Task<HttpRequestMessage> BuildRequestAsync(BotContext context, TRequest request, CancellationToken cancellationToken);

    protected abstract Task<TResponse> ParseResponseAsync(BotContext context, TRequest request, HttpResponseMessage response, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken);
}
