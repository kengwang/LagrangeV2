using Lagrange.Core.Internal.Context;
using Lagrange.Core.Events;
using System.Net.Http.Headers;
using Lagrange.Core.Internal.Http;

namespace Lagrange.Core.Services;

/// <summary>Base class for QQ web services that use the BotContext HTTP session.</summary>
public abstract class HttpService<TRequest, TResponse> : IHttpService
    where TRequest : ProtocolEvent
    where TResponse : ProtocolEvent
{
    private static readonly HttpRequestOptionsKey<bool> AuthPrepared = new("Lagrange.HttpService.AuthPrepared");
    private readonly IReadOnlyList<string> _cookieDomains;
    private HttpServiceAttribute? _metadata;
    void IHttpService.Configure(HttpServiceAttribute metadata) => _metadata = metadata;
    private HttpServiceAttribute Metadata => _metadata ?? throw new InvalidOperationException("HTTP service metadata must be configured by the generated registry.");

    protected HttpService(params string[] cookieDomains)
    {
        _cookieDomains = cookieDomains ?? throw new ArgumentNullException(nameof(cookieDomains));
    }

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
        if ((metadata.AuthInjection & (HttpAuthInjection.UrlBknFromPSkey | HttpAuthInjection.FormBknFromPSkey | HttpAuthInjection.FormCredentials)) != 0 || metadata.BodyPSkeyPath is not null)
            pskey = await context.HttpSessionContext.GetPSkeyAsync(domains[0], ct);
        if ((metadata.AuthInjection & (HttpAuthInjection.UrlBknFromSkey | HttpAuthInjection.FormBknFromSkey | HttpAuthInjection.FormCredentials)) != 0)
            skey = await context.HttpSessionContext.GetSkeyAsync(ct);
        await HttpRequestAuthentication.ApplyAsync(message, metadata, pskey, skey, ct);
        message.Options.Set(AuthPrepared, true);
    }

    protected async Task<HttpRequestMessage> CreateGetRequestAsync(BotContext context, Uri url, CancellationToken ct)
    {
        var request = CreateGetRequest(url);
        await PrepareRequestAsync(context, request, ct);
        return request;
    }

    protected async Task<HttpRequestMessage> CreateFormRequestAsync(BotContext context, HttpMethod method, Uri url, IEnumerable<KeyValuePair<string, string?>> fields, CancellationToken ct)
    {
        var request = CreateFormRequest(method, url, fields);
        await PrepareRequestAsync(context, request, ct);
        return request;
    }

    protected async Task<HttpRequestMessage> CreateRequestAsync(BotContext context, HttpMethod method, Uri url, HttpContent content, CancellationToken ct)
    {
        var request = CreateRequest(method, url, content);
        await PrepareRequestAsync(context, request, ct);
        return request;
    }

    protected async Task<HttpRequestMessage> CreateJsonRequestAsync(BotContext context, HttpMethod method, Uri url, ReadOnlyMemory<byte> payload, CancellationToken ct)
    {
        var request = CreateJsonRequest(method, url, payload);
        await PrepareRequestAsync(context, request, ct);
        return request;
    }

    // Binary ticket protocols embed their authentication inside a protobuf payload.
    protected Task<string> GetTicketAsync(BotContext context, CancellationToken ct) =>
        context.HttpSessionContext.GetPSkeyAsync(Metadata.CookieDomains.FirstOrDefault() ?? "collector.weiyun.com", ct);

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
    {
        var values = (fields ?? []).ToDictionary(pair => pair.Key, pair => pair.Value ?? string.Empty, StringComparer.Ordinal);
        return CreateRequest(method, url, new FormUrlEncodedContent(values));
    }

    protected HttpRequestMessage CreateJsonRequest(HttpMethod method, Uri? url = null, ReadOnlyMemory<byte> payload = default)
    {
        var content = new ByteArrayContent(payload.ToArray());
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        return CreateRequest(method, url, content);
    }

    Type IHttpService.RequestType => typeof(TRequest);

    async ValueTask<ProtocolEvent> IHttpService.ExecuteAsync(BotContext context, ProtocolEvent request, CancellationToken cancellationToken) =>
        await ExecuteAsync(context, (TRequest)request, cancellationToken);

    protected abstract Task<HttpRequestMessage> BuildRequestAsync(BotContext context, TRequest request, CancellationToken cancellationToken);

    protected abstract Task<TResponse> ParseResponseAsync(BotContext context, TRequest request, HttpResponseMessage response, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken);
}
