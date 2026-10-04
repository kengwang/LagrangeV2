using Lagrange.Core.Internal.Context;
using Lagrange.Core.Events;
using Lagrange.Core.Internal.Http;

namespace Lagrange.Core.Services;

/// <summary>Base class for QQ web services that use the BotContext HTTP session.</summary>
public abstract class HttpService<TRequest, TResponse> : IHttpService
    where TRequest : ProtocolEvent
    where TResponse : ProtocolEvent
{
    private static readonly HttpRequestOptionsKey<bool> AuthPrepared = new("Lagrange.HttpService.AuthPrepared");
    private HttpServiceAttribute? _metadata;
    void IHttpService.Configure(HttpServiceAttribute metadata) => _metadata = metadata;
    private HttpServiceAttribute Metadata => _metadata ?? throw new InvalidOperationException("HTTP service metadata must be configured by the generated registry.");

    protected HttpService() { }

    public virtual async Task<TResponse> ExecuteAsync(BotContext context, TRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        var session = context.HttpSessionContext;
        using var httpRequest = await BuildRequestAsync(context, request, cancellationToken);
        if (!httpRequest.Options.TryGetValue(AuthPrepared, out var prepared) || !prepared)
            await PrepareRequestAsync(context, httpRequest, cancellationToken);
        using var response = await session.SendAsync(httpRequest, cancellationToken);
        var payload = await session.ReadResponseAsync(response, cancellationToken);
        return await ParseResponseAsync(context, request, response, payload, cancellationToken);
    }

    protected async Task PrepareRequestAsync(BotContext context, HttpRequestMessage message, CancellationToken ct)
    {
        var metadata = Metadata;
        var domains = metadata.CookieDomains;
        await context.HttpSessionContext.PrepareRequestAsync(message, domains, ct);
        string? pskey = null;
        string? skey = null;
        if (metadata.RequiresPSkey)
            pskey = await context.HttpSessionContext.GetPSkeyAsync(domains[0], ct);
        if (metadata.RequiresSkey)
            skey = await context.HttpSessionContext.GetSkeyAsync(ct);
        await HttpRequestAuthentication.ApplyAsync(message, metadata, pskey, skey, ct);
        message.Options.Set(AuthPrepared, true);
    }

    protected async Task<HttpRequestMessage> CreateGetRequestAsync(BotContext context, Uri url, CancellationToken ct)
        => await CreatePreparedRequestAsync(context, HttpMethod.Get, url, null, ct);

    protected async Task<HttpRequestMessage> CreateFormRequestAsync(BotContext context, HttpMethod method, Uri url, IEnumerable<KeyValuePair<string, string?>> fields, CancellationToken ct)
        => await CreatePreparedRequestAsync(context, method, url, CreateFormContent(fields), ct);

    protected async Task<HttpRequestMessage> CreateRequestAsync(BotContext context, HttpMethod method, Uri url, HttpContent content, CancellationToken ct)
        => await CreatePreparedRequestAsync(context, method, url, content, ct);

    protected async Task<HttpRequestMessage> CreateJsonRequestAsync(BotContext context, HttpMethod method, Uri url, ReadOnlyMemory<byte> payload, CancellationToken ct)
        => await CreatePreparedRequestAsync(context, method, url, CreateJsonContent(payload), ct);

    private async Task<HttpRequestMessage> CreatePreparedRequestAsync(BotContext context, HttpMethod method, Uri url, HttpContent? content, CancellationToken ct)
    {
        var request = CreateRequest(method, url, content);
        await PrepareRequestAsync(context, request, ct);
        return request;
    }

    // Binary ticket protocols embed their authentication inside a protobuf payload.
    protected Task<string> GetTicketAsync(BotContext context, CancellationToken ct) =>
        context.HttpSessionContext.GetPSkeyAsync(Metadata.CookieDomains.FirstOrDefault() ?? "weiyun.com", ct);

    protected async Task<ReadOnlyMemory<byte>> SendRequestAsync(BotContext context, HttpRequestMessage message, CancellationToken cancellationToken)
    {
        await PrepareRequestAsync(context, message, cancellationToken);
        using var response = await context.HttpSessionContext.SendAsync(message, cancellationToken);
        return await context.HttpSessionContext.ReadResponseAsync(response, cancellationToken);
    }

    protected HttpRequestMessage CreateRequest(HttpMethod method, Uri? url = null, HttpContent? content = null)
    {
        url ??= new Uri(Metadata.Path, UriKind.Absolute);
        return new HttpRequestMessage(method, url) { Content = content };
    }

    protected HttpRequestMessage CreateGetRequest(Uri? url = null) => CreateRequest(HttpMethod.Get, url);

    protected HttpRequestMessage CreateFormRequest(HttpMethod method, Uri? url = null, IEnumerable<KeyValuePair<string, string?>>? fields = null)
        => CreateRequest(method, url, CreateFormContent(fields));

    private static FormUrlEncodedContent CreateFormContent(IEnumerable<KeyValuePair<string, string?>>? fields)
    {
        var values = (fields ?? []).ToDictionary(pair => pair.Key, pair => pair.Value ?? string.Empty, StringComparer.Ordinal);
        return new FormUrlEncodedContent(values);
    }

    protected HttpRequestMessage CreateJsonRequest(HttpMethod method, Uri? url = null, ReadOnlyMemory<byte> payload = default)
        => CreateRequest(method, url, CreateJsonContent(payload));

    private static ByteArrayContent CreateJsonContent(ReadOnlyMemory<byte> payload)
    {
        var content = new ByteArrayContent(payload.ToArray());
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        return content;
    }

    Type IHttpService.RequestType => typeof(TRequest);

    async ValueTask<ProtocolEvent> IHttpService.ExecuteAsync(BotContext context, ProtocolEvent request, CancellationToken cancellationToken) =>
        await ExecuteAsync(context, (TRequest)request, cancellationToken);

    protected abstract Task<HttpRequestMessage> BuildRequestAsync(BotContext context, TRequest request, CancellationToken cancellationToken);

    protected abstract Task<TResponse> ParseResponseAsync(BotContext context, TRequest request, HttpResponseMessage response, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken);
}
