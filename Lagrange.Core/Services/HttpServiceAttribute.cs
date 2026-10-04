namespace Lagrange.Core.Services;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class HttpServiceAttribute(string name, string method, string path, params string[] cookieDomains) : Attribute
{
    public string Name { get; } = name;
    public string Method { get; } = method;
    public string Path { get; } = path;
    public IReadOnlyList<string> CookieDomains { get; } = cookieDomains is { Length: > 0 } ? [.. cookieDomains] : [];

    public HttpAuthInjection AuthInjection { get; init; }

    public string UrlTokenName { get; init; } = "g_tk";

    public string BodyTokenName { get; init; } = "bkn";

    public string? BodyPSkeyPath { get; init; }

    internal bool RequiresPSkey =>
        (AuthInjection & (HttpAuthInjection.UrlBknFromPSkey | HttpAuthInjection.FormBknFromPSkey | HttpAuthInjection.FormCredentials)) != 0 ||
        BodyPSkeyPath is not null;

    internal bool RequiresSkey =>
        (AuthInjection & (HttpAuthInjection.UrlBknFromSkey | HttpAuthInjection.FormBknFromSkey | HttpAuthInjection.FormCredentials)) != 0;
}
