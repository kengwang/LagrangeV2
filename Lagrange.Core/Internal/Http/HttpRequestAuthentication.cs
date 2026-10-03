using System.Text;
using System.Text.Json.Nodes;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Http;

internal static class HttpRequestAuthentication
{
    internal static async Task ApplyAsync(HttpRequestMessage request, HttpServiceAttribute metadata, string? pskey, string? skey, CancellationToken ct)
    {
        var injection = metadata.AuthInjection;
        string Token(bool fromSkey) => ComputeBkn((fromSkey ? skey : pskey) ?? throw new InvalidOperationException("Required HTTP authentication key is unavailable."));
        if ((injection & (HttpAuthInjection.UrlBknFromPSkey | HttpAuthInjection.UrlBknFromSkey)) != 0)
            request.RequestUri = SetQuery(request.RequestUri!, metadata.UrlTokenName, Token((injection & HttpAuthInjection.UrlBknFromSkey) != 0));
        if (metadata.BodyPSkeyPath is { } path && request.Content?.Headers.ContentType?.MediaType == "application/json")
        {
            if (request.Content?.Headers.ContentType?.MediaType != "application/json") throw new InvalidOperationException("Body ticket injection requires JSON content.");
            var old = request.Content;
            var json = JsonNode.Parse(await old.ReadAsStringAsync(ct)) ?? throw new InvalidOperationException("JSON request is empty.");
            var keys = path.Split('.');
            var target = json;
            foreach (var key in keys[..^1]) target = (target is JsonArray array ? array[int.Parse(key, global::System.Globalization.CultureInfo.InvariantCulture)] : target[key]) ?? throw new InvalidOperationException("JSON authentication path is missing.");
            target[keys[^1]] = pskey ?? throw new InvalidOperationException("Required p_skey is unavailable.");
            request.Content = new StringContent(json.ToJsonString(), Encoding.UTF8, "application/json");
            old.Dispose();
        }
        if ((injection & (HttpAuthInjection.FormBknFromPSkey | HttpAuthInjection.FormBknFromSkey | HttpAuthInjection.FormCredentials)) == 0) return;
        var additions = new List<KeyValuePair<string, string>>();
        if ((injection & (HttpAuthInjection.FormBknFromPSkey | HttpAuthInjection.FormBknFromSkey)) != 0)
            additions.Add(new(metadata.BodyTokenName, Token((injection & HttpAuthInjection.FormBknFromSkey) != 0)));
        if ((injection & HttpAuthInjection.FormCredentials) != 0)
        {
            additions.Add(new("p_skey", pskey ?? throw new InvalidOperationException("Required p_skey is unavailable.")));
            additions.Add(new("skey", skey ?? throw new InvalidOperationException("Required skey is unavailable.")));
        }
        if (request.Content is MultipartFormDataContent multipart)
        {
            foreach (var addition in additions)
            {
                if (multipart.Any(part => part.Headers.ContentDisposition?.Name?.Trim('"') == addition.Key))
                    throw new InvalidOperationException("Service must not supply an automatically injected authentication field.");
                multipart.Add(new StringContent(addition.Value), addition.Key);
            }
        }
        else if (request.Content?.Headers.ContentType?.MediaType == "application/x-www-form-urlencoded")
        {
            var previous = request.Content;
            var fields = ParseQuery(await previous.ReadAsStringAsync(ct));
            fields.RemoveAll(field => additions.Any(addition => addition.Key == field.Key));
            fields.AddRange(additions);
            request.Content = new FormUrlEncodedContent(fields);
            previous.Dispose();
        }
        else throw new InvalidOperationException("Automatic form authentication requires URL-encoded or multipart content.");
    }

    internal static Uri SetQuery(Uri uri, string name, string value)
    {
        var builder = new UriBuilder(uri);
        var fields = ParseQuery(builder.Query.TrimStart('?'));
        fields.RemoveAll(field => field.Key == name);
        fields.Add(new(name, value));
        builder.Query = string.Join("&", fields.Select(pair => $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value)}"));
        return builder.Uri;
    }

    private static List<KeyValuePair<string, string>> ParseQuery(string text) => text.Split('&', StringSplitOptions.RemoveEmptyEntries).Select(field =>
    {
        var parts = field.Split('=', 2);
        return new KeyValuePair<string, string>(Uri.UnescapeDataString(parts[0].Replace("+", " ")), Uri.UnescapeDataString((parts.Length == 2 ? parts[1] : "").Replace("+", " ")));
    }).ToList();

    internal static string ComputeBkn(string value)
    {
        uint hash = 5381;
        foreach (var c in value) hash = unchecked(hash + (hash << 5) + c);
        return (hash & 0x7FFFFFFF).ToString(global::System.Globalization.CultureInfo.InvariantCulture);
    }
}
