using System.Text.Json;
using Lagrange.Core.Exceptions;

namespace Lagrange.Core.Internal.Http;

internal static class HttpResponseParser
{
    public static JsonDocument ParseJson(ReadOnlyMemory<byte> payload, string service)
    {
        if (payload.IsEmpty) throw new HttpServiceException(service, "HTTP response body is empty.");
        try { return JsonDocument.Parse(payload); }
        catch (JsonException exception) { throw new HttpServiceException(service, "HTTP response is not valid JSON.", innerException: exception); }
    }

    public static JsonDocument ParseJsonp(ReadOnlyMemory<byte> payload, string service)
    {
        if (payload.IsEmpty) throw new HttpServiceException(service, "HTTP response body is empty.");
        var text = System.Text.Encoding.UTF8.GetString(payload.Span).Trim();
        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        if (start < 0 || end <= start) throw new HttpServiceException(service, "HTTP response is not valid JSONP.");
        return ParseJson(System.Text.Encoding.UTF8.GetBytes(text[start..(end + 1)]), service);
    }

    public static void EnsureBusinessSuccess(JsonElement root, string service, string codeProperty = "code", string messageProperty = "message")
    {
        if (!root.TryGetProperty(codeProperty, out var code)) return;
        var value = code.ValueKind == JsonValueKind.Number && code.TryGetInt32(out var number) ? number : 0;
        if (value == 0) return;
        var message = root.TryGetProperty(messageProperty, out var text) ? text.ToString() : "HTTP service returned a business error.";
        throw new HttpServiceException(service, message, businessCode: value);
    }
}
