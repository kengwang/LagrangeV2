namespace Lagrange.Core.Services;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class HttpServiceAttribute(string name, string method, string path, params string[] cookieDomains) : Attribute
{
    public string Name { get; } = name;
    public string Method { get; } = method;
    public string Path { get; } = path;
    public IReadOnlyList<string> CookieDomains { get; } = cookieDomains;
}
