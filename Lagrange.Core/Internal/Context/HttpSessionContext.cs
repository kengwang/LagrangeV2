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
    private readonly ConcurrentDictionary<string, Lazy<Task<string>>> _skeyCache = new(StringComparer.Ordinal);

    public HttpSessionContext(BotContext context, HttpMessageHandler? handler = null)
    {
        _context = context;
        _client = new HttpClient(handler ?? new HttpClientHandler { CookieContainer = _cookies, UseCookies = true })
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
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
        // The handler owns the Cookie header. The web endpoint expects cookies
        // from one jump exchange as a single jar; manually adding a second Cookie
        // header here makes HttpClient append the jar a second time and can
        // leave Qzone with two different p_skey values.
        var sourceDomains = domains.Select(CookieSourceDomain).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var destination = message.RequestUri?.Host ?? sourceDomains[0];
        foreach (var domain in sourceDomains)
        {
            if (_cookies.GetCookies(new Uri($"https://{domain}/"))["p_skey"] is null)
                SetCookie(domain, "p_skey", pSkeys[Array.IndexOf(sourceDomains, domain)]);
            if (_cookies.GetCookies(new Uri($"https://{destination}/"))["p_skey"]?.Value != pSkeys[Array.IndexOf(sourceDomains, domain)])
                SetCookie(destination, "p_skey", pSkeys[Array.IndexOf(sourceDomains, domain)]);
            // ptlogin2 may set auxiliary cookies on ssl.ptlogin2.qq.com or
            // .qq.com. Forward the complete jump result to the destination,
            // mirroring those non-ticket cookies into the
            // destination jar instead of relying on domain matching.
            foreach (var origin in new[] { "ssl.ptlogin2.qq.com", "qq.com", "qzone.qq.com", "vip.qq.com", "qun.qq.com" })
            foreach (Cookie cookie in _cookies.GetCookies(new Uri($"https://{origin}/")))
            {
                if (cookie.Name is "p_skey" or "skey") continue;
                if (_cookies.GetCookies(new Uri($"https://{domain}/"))[cookie.Name] is null)
                    SetCookie(domain, cookie.Name, cookie.Value);
                if (_cookies.GetCookies(new Uri($"https://{destination}/"))[cookie.Name] is null)
                    SetCookie(destination, cookie.Name, cookie.Value);
            }
            if (_cookies.GetCookies(new Uri($"https://{domain}/"))["p_uin"] is null)
                SetCookie(domain, "p_uin", $"o{_context.BotUin}");
            if (_cookies.GetCookies(new Uri($"https://{domain}/"))["uin"] is null)
                SetCookie(domain, "uin", $"o{_context.BotUin}");
            if (_cookies.GetCookies(new Uri($"https://{destination}/"))["p_uin"] is null)
                SetCookie(destination, "p_uin", $"o{_context.BotUin}");
            if (_cookies.GetCookies(new Uri($"https://{destination}/"))["uin"] is null)
                SetCookie(destination, "uin", $"o{_context.BotUin}");
        }
    }

    internal async Task<string> GetPSkeyAsync(string domain, CancellationToken cancellationToken)
    {
        // The cookie jar owns expiry and rotation; tasks only coalesce concurrent logins.
        if (_cookies.GetCookies(new Uri($"https://{domain}/"))["p_skey"] is { Value.Length: > 0 } existing)
            return existing.Value;
        var entry = _pSkeyCache.GetOrAdd(domain, key => new Lazy<Task<string>>(() => FetchPSkeyAsync(key, cancellationToken), LazyThreadSafetyMode.ExecutionAndPublication));
        try
        {
            var value = await entry.Value.WaitAsync(cancellationToken);
            // Other ptlogin jumps can rotate this cookie after the cached task completes.
            return _cookies.GetCookies(new Uri($"https://{domain}/"))["p_skey"] is { Value.Length: > 0 } current
                ? current.Value : value;
        }
        finally
        {
            _pSkeyCache.TryRemove(new KeyValuePair<string, Lazy<Task<string>>>(domain, entry));
        }
    }

    internal async Task<string> GetBknFromPSkeyAsync(string domain, CancellationToken cancellationToken) =>
        ComputeBkn(await GetPSkeyAsync(domain, cancellationToken));

    internal async Task<string> GetBknFromSkeyAsync(CancellationToken cancellationToken) => ComputeBkn(await GetSkeyAsync(cancellationToken));

    private static string ComputeBkn(string value)
    {
        uint hash = 5381;
        foreach (var c in value) hash += (hash << 5) + c;
        return (hash & 0x7FFFFFFF).ToString();
    }

    internal async Task<string> GetSkeyAsync(CancellationToken cancellationToken)
    {
        if (FindSkey() is { } existing) return existing;
        var entry = _skeyCache.GetOrAdd("skey", _ => new Lazy<Task<string>>(() => FetchSkeyAsync(cancellationToken), LazyThreadSafetyMode.ExecutionAndPublication));
        try { return FindSkey() ?? await entry.Value.WaitAsync(cancellationToken); }
        finally { _skeyCache.TryRemove(new KeyValuePair<string, Lazy<Task<string>>>("skey", entry)); }
    }

    private string? FindSkey()
    {
        foreach (var domain in new[] { "ssl.ptlogin2.qq.com", "qq.com", "qzone.qq.com", "qun.qq.com" })
        {
            var cookie = _cookies.GetCookies(new Uri($"https://{domain}/"))["skey"];
            if (cookie is not null && !string.IsNullOrWhiteSpace(cookie.Value)) return cookie.Value;
        }
        return null;
    }

    private async Task<string> FetchSkeyAsync(CancellationToken cancellationToken)
    {
        var clientKey = await _context.EventContext.SendEvent<FetchClientKeyEventResp>(new FetchClientKeyEventReq(), cancellationToken);
        if (string.IsNullOrWhiteSpace(clientKey.ClientKey)) return string.Empty;
        var jumpTarget = Uri.EscapeDataString("https://h5.qzone.qq.com/qqnt/qzoneinpcqq/friend?refresh=0&clientuin=0&darkMode=0");
        var url = $"https://ssl.ptlogin2.qq.com/jump?ptlang=1033&clientuin={_context.BotUin}&clientkey={Uri.EscapeDataString(clientKey.ClientKey)}&u1={jumpTarget}&keyindex={clientKey.KeyType}";
        using var response = await _client.GetAsync(url, cancellationToken);
        // ptlogin2 may answer with a redirect or a non-success status while
        // still setting the cookie jar. The cookie, not the body status, is
        // the result of this exchange.
        return FindSkey() ?? string.Empty;
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

    internal void InvalidateCookies(string domain)
    {
        _pSkeyCache.TryRemove(domain, out _);
        bool clearSkey = domain.Equals("qzone.qq.com", StringComparison.OrdinalIgnoreCase);
        if (clearSkey) _skeyCache.TryRemove("skey", out _);
        foreach (Cookie cookie in _cookies.GetAllCookies())
        {
            var host = cookie.Domain.TrimStart('.');
            if (clearSkey && cookie.Name == "skey" || cookie.Name == "p_skey" && (host.Equals(domain, StringComparison.OrdinalIgnoreCase)
                || host.EndsWith($".{domain}", StringComparison.OrdinalIgnoreCase)
                || CookieSourceDomain(host).Equals(domain, StringComparison.OrdinalIgnoreCase)))
                cookie.Expired = true;
        }
    }

    private async Task<string> FetchPSkeyAsync(string domain, CancellationToken cancellationToken)
    {
        // Match the web client flow: the ptlogin jump populates the cookie jar
        // for the requested domain before falling back to OIDB p_skey.
        var clientKey = await _context.EventContext.SendEvent<FetchClientKeyEventResp>(new FetchClientKeyEventReq(), cancellationToken);
        if (!string.IsNullOrWhiteSpace(clientKey.ClientKey))
        {
            var jumpTarget = Uri.EscapeDataString($"https://{domain}/{_context.BotUin}/infocenter");
            var url = $"https://ssl.ptlogin2.qq.com/jump?ptlang=1033&clientuin={_context.BotUin}&clientkey={Uri.EscapeDataString(clientKey.ClientKey)}&u1={jumpTarget}&keyindex={clientKey.KeyType}";
            using var response = await _client.GetAsync(url, cancellationToken);
            if (_cookies.GetCookies(new Uri($"https://{domain}/"))["p_skey"] is { Value.Length: > 0 } jumped)
            {
                return jumped.Value;
            }
        }
        var cookieResponse = await _context.EventContext.SendEvent<FetchCookiesEventResp>(new FetchCookiesEventReq([domain]), cancellationToken);
        cookieResponse.Cookies.TryGetValue(domain, out var oidbPskey);
        if (!string.IsNullOrWhiteSpace(oidbPskey))
        {
            SetCookie(domain, "p_skey", oidbPskey);
            return oidbPskey;
        }
        if (_cookies.GetCookies(new Uri($"https://{domain}/"))["p_skey"] is { Value.Length: > 0 } cookie) return cookie.Value;
        throw new Lagrange.Core.Exceptions.HttpServiceException(domain, "Cookie service did not return p_skey.");
    }

    internal async Task<ReadOnlyMemory<byte>> ReadResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            await using var errorStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var preview = new byte[4096];
            int length = await errorStream.ReadAtLeastAsync(preview, preview.Length, throwOnEndOfStream: false, cancellationToken);
            string body = System.Text.Encoding.UTF8.GetString(preview, 0, length);
            var uri = response.RequestMessage?.RequestUri;
            throw new Lagrange.Core.Exceptions.HttpServiceException(uri?.GetLeftPart(UriPartial.Path) ?? "http", $"HTTP request failed with status {(int)response.StatusCode}.", (int)response.StatusCode) { ResponseBody = body };
        }
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
