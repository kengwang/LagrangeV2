using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Lagrange.Milky.Configurations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Lagrange.Milky.Security;

public sealed class MilkyAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string? token = Context.RequestServices.GetRequiredService<MilkyConfiguration>().AccessToken;
        if (string.IsNullOrEmpty(token))
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(
                new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "milky")], Scheme.Name)), Scheme.Name)));

        string supplied = Request.Headers.Authorization.ToString();
        if (AuthenticationHeaderValue.TryParse(supplied, out var header)
            && string.Equals(header.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase)
            && string.Equals(header.Parameter, token, StringComparison.Ordinal) || string.Equals(Request.Query["access_token"], token, StringComparison.Ordinal))
            return Success();

        return Task.FromResult(AuthenticateResult.Fail("Invalid access token."));

        Task<AuthenticateResult> Success() => Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(
            new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "milky")], Scheme.Name)), Scheme.Name)));
    }
}
