using Lagrange.Core.Common;
using System.Net.Http.Headers;
using Lagrange.Core.Events;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.Http;

[EventSubscribe<RefreshHttpCookiesEventReq>(Protocols.All)]
[HttpService("http.refresh_cookies", "INTERNAL", "/")]
internal sealed class RefreshHttpCookiesService : IHttpService
{
    public Type RequestType => typeof(RefreshHttpCookiesEventReq);

    public async ValueTask<ProtocolEvent> ExecuteAsync(BotContext context, ProtocolEvent request, CancellationToken cancellationToken)
    {
        var domains = ((RefreshHttpCookiesEventReq)request).Domains;
        if (domains.Length == 0) throw new ArgumentException("At least one cookie domain is required.", nameof(request));
        foreach (var domain in domains)
        {
            context.HttpSessionContext.InvalidateCookies(domain);
            await context.HttpSessionContext.GetPSkeyAsync(domain, cancellationToken);
        }
        return new RefreshHttpCookiesEventResp();
    }
}

[HttpService("http.post_bytes", "POST", "/")]
[EventSubscribe<PostHttpBytesEventReq>(Protocols.All)]
internal sealed class PostHttpBytesService : HttpService<PostHttpBytesEventReq, PostHttpBytesEventResp>
{
    protected override Task<HttpRequestMessage> BuildRequestAsync(BotContext context, PostHttpBytesEventReq request, CancellationToken cancellationToken)
    {
        var message = new HttpRequestMessage(HttpMethod.Post, request.Endpoint) { Content = new ByteArrayContent(request.Payload.ToArray()) };
        message.Content.Headers.ContentType = new MediaTypeHeaderValue(request.ContentType);
        foreach (var header in request.Headers ?? new Dictionary<string, string>())
            if (!message.Headers.TryAddWithoutValidation(header.Key, header.Value)) message.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        return Task.FromResult(message);
    }

    protected override Task<PostHttpBytesEventResp> ParseResponseAsync(BotContext context, PostHttpBytesEventReq request, HttpResponseMessage response, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken) =>
        Task.FromResult(new PostHttpBytesEventResp(payload.ToArray()));
}
