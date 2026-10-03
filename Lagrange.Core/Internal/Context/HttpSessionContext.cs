using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Collections.Concurrent;
using Lagrange.Core.Internal.Events.System;

namespace Lagrange.Core.Internal.Context;

internal sealed class HttpSessionContext : IDisposable
{
    private readonly BotContext _context;
    private readonly HttpClient _client;
    private readonly CookieContainer _cookies = new();
    private const int MaxResponseBytes = 16 * 1024 * 1024;
    private readonly ConcurrentDictionary<string, Lazy<Task<string>>> _pSkeyCache = new(StringComparer.OrdinalIgnoreCase);

    public HttpSessionContext(BotContext context, HttpMessageHandler? handler = null)
    {
        _context = context;
        _client = new HttpClient(handler ?? new HttpClientHandler { CookieContainer = _cookies, UseCookies = true })
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
        _client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("LagrangeV2", "1.0"));
    }

    public TimeSpan Timeout { get => _client.Timeout; set => _client.Timeout = value; }

    internal async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            return await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new Lagrange.Core.Exceptions.HttpServiceException(request.RequestUri?.Host ?? "http", "HTTP request timed out.", innerException: new TimeoutException());
        }
        catch (HttpRequestException exception)
        {
            throw new Lagrange.Core.Exceptions.HttpServiceException(request.RequestUri?.Host ?? "http", "HTTP request failed.", innerException: exception);
        }
    }

    internal async Task PrepareRequestAsync(HttpRequestMessage message, IReadOnlyList<string> domains, CancellationToken cancellationToken)
    {
        if (domains.Count == 0) return;
        var pSkeys = await Task.WhenAll(domains.Select(CookieSourceDomain).Distinct(StringComparer.OrdinalIgnoreCase).Select(domain => GetPSkeyAsync(domain, cancellationToken)));
        var cookies = new List<string>();
        foreach (var pskey in pSkeys) cookies.Add($"p_skey={pskey}");
        if (_context.Keystore.WLoginSigs.SKey is { Length: > 0 } skey) cookies.Add($"skey={System.Text.Encoding.UTF8.GetString(skey)}");
        cookies.Add($"uin=o{_context.BotUin}");
        message.Headers.TryAddWithoutValidation("Cookie", string.Join("; ", cookies));
    }

    internal async Task<string> GetPSkeyAsync(string domain, CancellationToken cancellationToken)
    {
        var entry = _pSkeyCache.GetOrAdd(domain, key => new Lazy<Task<string>>(() => FetchPSkeyAsync(key, cancellationToken), LazyThreadSafetyMode.ExecutionAndPublication));
        try { return await entry.Value.WaitAsync(cancellationToken); }
        catch
        {
            _pSkeyCache.TryRemove(new KeyValuePair<string, Lazy<Task<string>>>(domain, entry));
            throw;
        }
    }

    private static string CookieSourceDomain(string domain) => domain switch
    {
        "web.qun.qq.com" => "qun.qq.com",
        "h5.qzone.qq.com" => "qzone.qq.com",
        "up.qzone.qq.com" => "qzone.qq.com",
        "u.photo.qzone.qq.com" => "qzone.qq.com",
        "ic2.qzone.qq.com" => "qzone.qq.com",
        "taotao.qzone.qq.com" => "qzone.qq.com",
        "w.qzone.qq.com" => "qzone.qq.com",
        _ => domain,
    };

    internal void InvalidateCookies(string domain) => _pSkeyCache.TryRemove(domain, out _);

    private async Task<string> FetchPSkeyAsync(string domain, CancellationToken cancellationToken)
    {
        var response = await _context.EventContext.SendEvent<FetchCookiesEventResp>(new FetchCookiesEventReq([domain]), cancellationToken);
        if (response.Cookies.TryGetValue(domain, out var pskey) && !string.IsNullOrWhiteSpace(pskey)) return pskey;

        var clientKey = await _context.EventContext.SendEvent<FetchClientKeyEventResp>(new FetchClientKeyEventReq(), cancellationToken);
        if (string.IsNullOrWhiteSpace(clientKey.ClientKey)) throw new Lagrange.Core.Exceptions.HttpServiceException(domain, "Required p_skey and client key are unavailable.");
        var target = Uri.EscapeDataString($"https://{domain}/{_context.BotUin}/infocenter");
        var jump = new Uri($"https://ssl.ptlogin2.qq.com/jump?ptlang=1033&clientuin={_context.BotUin}&clientkey={Uri.EscapeDataString(clientKey.ClientKey)}&u1={target}&keyindex={clientKey.KeyType}");
        using var request = new HttpRequestMessage(HttpMethod.Get, jump);
        using var httpResponse = await SendAsync(request, cancellationToken);
        _ = await ReadResponseAsync(httpResponse, cancellationToken);
        if (_cookies.GetCookies(new Uri($"https://{domain}/"))["p_skey"] is { Value.Length: > 0 } cookie) return cookie.Value;
        throw new Lagrange.Core.Exceptions.HttpServiceException(domain, "Cookie exchange did not return p_skey.");
    }

    internal async Task<ReadOnlyMemory<byte>> ReadResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode) throw new Lagrange.Core.Exceptions.HttpServiceException(response.RequestMessage?.RequestUri?.Host ?? "http", $"HTTP request failed with status {(int)response.StatusCode}.", (int)response.StatusCode);
        if (response.Content.Headers.ContentLength is > MaxResponseBytes) throw new Lagrange.Core.Exceptions.HttpServiceException(response.RequestMessage?.RequestUri?.Host ?? "http", "HTTP response exceeds the configured size limit.", (int)response.StatusCode);
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var buffer = new MemoryStream();
        var chunk = new byte[81920];
        while (true)
        {
            var read = await stream.ReadAsync(chunk, cancellationToken);
            if (read == 0) break;
            if (buffer.Length + read > MaxResponseBytes) throw new Lagrange.Core.Exceptions.HttpServiceException(response.RequestMessage?.RequestUri?.Host ?? "http", "HTTP response exceeds the configured size limit.", (int)response.StatusCode);
            await buffer.WriteAsync(chunk.AsMemory(0, read), cancellationToken);
        }
        return buffer.ToArray();
    }

    public void SetCookies(IEnumerable<KeyValuePair<string, string>> cookies, string domain)
    {
        foreach (var cookie in cookies)
        {
            // FetchCookiesService returns a domain -> p_skey map.
            SetCookie(domain, cookie.Key.Equals(domain, StringComparison.OrdinalIgnoreCase) ? "p_skey" : cookie.Key, cookie.Value);
        }
    }

    public void SetCookie(string domain, string name, string value)
    {
        if (string.IsNullOrWhiteSpace(domain) || string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Cookie domain and name are required.");
        _cookies.SetCookies(new Uri(domain.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? domain : $"https://{domain}"), $"{name}={value}");
    }

    public async Task<JsonDocument> GetJsonAsync(Uri uri, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(new HttpRequestMessage(HttpMethod.Get, uri), cancellationToken);
        response.EnsureSuccessStatusCode();
        if (response.Content.Headers.ContentLength is > MaxResponseBytes) throw new InvalidDataException("HTTP response exceeds the configured size limit.");
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        try
        {
            return await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException($"HTTP response from '{uri.Host}' is not valid JSON.", exception);
        }
    }

    public async Task<string> GetTextAsync(Uri uri, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(new HttpRequestMessage(HttpMethod.Get, uri), cancellationToken);
        response.EnsureSuccessStatusCode();
        if (response.Content.Headers.ContentLength is > MaxResponseBytes) throw new InvalidDataException("HTTP response exceeds the configured size limit.");
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);
        var text = await reader.ReadToEndAsync(cancellationToken);
        if (text.Length > MaxResponseBytes) throw new InvalidDataException("HTTP response exceeds the configured size limit.");
        return text;
    }

    public async Task<JsonDocument> PostJsonAsync(Uri uri, HttpContent content, CancellationToken cancellationToken = default)
    {
        using var postRequest = new HttpRequestMessage(HttpMethod.Post, uri) { Content = content };
        using var response = await SendAsync(postRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
        if (response.Content.Headers.ContentLength is > MaxResponseBytes) throw new InvalidDataException("HTTP response exceeds the configured size limit.");
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var buffer = new MemoryStream();
        var chunk = new byte[81920];
        while (true)
        {
            var read = await stream.ReadAsync(chunk, cancellationToken);
            if (read == 0) break;
            if (buffer.Length + read > MaxResponseBytes) throw new InvalidDataException("HTTP response exceeds the configured size limit.");
            await buffer.WriteAsync(chunk.AsMemory(0, read), cancellationToken);
        }
        try
        {
            return JsonDocument.Parse(buffer.ToArray());
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException($"HTTP response from '{uri.Host}' is not valid JSON.", exception);
        }
    }

    public async Task<string> PostTextAsync(Uri uri, HttpContent content, CancellationToken cancellationToken = default)
    {
        using var postRequest = new HttpRequestMessage(HttpMethod.Post, uri) { Content = content };
        using var response = await SendAsync(postRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
        if (response.Content.Headers.ContentLength is > MaxResponseBytes) throw new InvalidDataException("HTTP response exceeds the configured size limit.");
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);
        var text = await reader.ReadToEndAsync(cancellationToken);
        if (text.Length > MaxResponseBytes) throw new InvalidDataException("HTTP response exceeds the configured size limit.");
        return text;
    }

    public async Task<byte[]> PostBytesAsync(Uri uri, ReadOnlyMemory<byte> payload, string? contentType = null, IReadOnlyDictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = new ByteArrayContent(payload.ToArray())
        };
        if (!string.IsNullOrWhiteSpace(contentType)) request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        if (headers is not null)
        {
            foreach (var header in headers)
            {
                if (!request.Headers.TryAddWithoutValidation(header.Key, header.Value))
                    request.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }
        using var response = await SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        if (response.Content.Headers.ContentLength is > MaxResponseBytes) throw new InvalidDataException("HTTP response exceeds the configured size limit.");
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var buffer = new MemoryStream();
        var chunk = new byte[81920];
        while (true)
        {
            var read = await stream.ReadAsync(chunk, cancellationToken);
            if (read == 0) break;
            if (buffer.Length + read > MaxResponseBytes) throw new InvalidDataException("HTTP response exceeds the configured size limit.");
            await buffer.WriteAsync(chunk.AsMemory(0, read), cancellationToken);
        }
        return buffer.ToArray();
    }

    public void Dispose() => _client.Dispose();
}
