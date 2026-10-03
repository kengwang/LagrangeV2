namespace Lagrange.Core.Services;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class HttpServiceAttribute(string name, string method, string path, params string[] cookieDomains) : Attribute
{
    public string Name { get; } = name;
    public string Method { get; } = method;
    public string Path { get; } = path;
    public IReadOnlyList<string> CookieDomains { get; } = cookieDomains;

    public HttpAuthInjection AuthInjection { get; init; }

    public string UrlTokenName { get; init; } = "g_tk";

    public string BodyTokenName { get; init; } = "bkn";

    public string? BodyPSkeyPath { get; init; }
}
