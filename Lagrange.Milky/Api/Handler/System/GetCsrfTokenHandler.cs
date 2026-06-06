using System.Text;
using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Milky.Api.Exception;

namespace Lagrange.Milky.Api.Handler.System;

[Api("get_csrf_token")]
public class GetCsrfTokenHandler(BotContext bot) : IEmptyParameterApiHandler<GetCsrfTokenResult>
{
    private readonly BotContext _bot = bot;

    public Task<GetCsrfTokenResult> HandleAsync(CancellationToken token)
    {
        string skey = Encoding.UTF8.GetString(_bot.Keystore.WLoginSigs.SKey);
        if (string.IsNullOrEmpty(skey)) throw new ApiException(-1, "skey not found");

        int hash = 5381;
        foreach (char c in skey)
        {
            hash += (hash << 5) + c;
        }

        return Task.FromResult(new GetCsrfTokenResult(hash & 0x7fffffff));
    }
}

public class GetCsrfTokenResult(int csrfToken)
{
    [JsonPropertyName("csrf_token")]
    public int CsrfToken { get; } = csrfToken;
}
