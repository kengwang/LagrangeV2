namespace Lagrange.Core.Services;

[Flags]
public enum HttpAuthInjection
{
    None = 0,
    UrlBknFromPSkey = 1,
    UrlBknFromSkey = 2,
    FormBknFromPSkey = 4,
    FormBknFromSkey = 8,
    FormCredentials = 16,
}
