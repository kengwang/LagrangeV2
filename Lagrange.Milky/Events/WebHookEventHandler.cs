using System.Net.Http.Headers;
using Lagrange.Milky.Configurations;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lagrange.Milky.Events;

public sealed class WebHookEventHandler(
    MilkyEventHub hub,
    MilkyConfiguration configuration,
    IHttpClientFactory clients,
    ILogger<WebHookEventHandler> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = configuration.Event.WebHook;
        if (settings is null || !settings.Enabled || settings.TargetUrls.Length == 0)
            return;

        using var subscription = hub.Subscribe();
        while (await subscription.Reader.WaitToReadAsync(stoppingToken))
            while (subscription.Reader.TryRead(out byte[]? payload))
                await Task.WhenAll(settings.TargetUrls.Select(url => SendAsync(url, payload, stoppingToken)));
    }

    private async Task SendAsync(string url, byte[] payload, CancellationToken ct)
    {
        for (int attempt = 0; attempt < 2; attempt++)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = new ByteArrayContent(payload) };
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                if (!string.IsNullOrEmpty(configuration.AccessToken))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", configuration.AccessToken);
                using var response = await clients.CreateClient(nameof(WebHookEventHandler)).SendAsync(request, ct);
                if (response.IsSuccessStatusCode) return;
            }
            catch (Exception ex) when (attempt == 1 || ex is OperationCanceledException)
            {
                logger.LogWarning(ex, "Failed to send Milky WebHook to {Url}", url);
            }
        }
    }
}
