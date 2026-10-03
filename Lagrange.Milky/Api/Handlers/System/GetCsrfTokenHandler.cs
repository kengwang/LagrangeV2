using System.Threading;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.System;

[ApiHandler("get_csrf_token")]
public sealed class GetCsrfTokenHandler(BotContext lagrange) : INoRequestApiHandler<GetCsrfTokenHandler.Result>
{
    public async ValueTask<MilkyApiResponse<Result>> HandleAsync(CancellationToken ct)
        => new(new Result(await lagrange.FetchCsrfToken(ct).WaitAsync(ct)));

    public sealed class Result(string token)
    {
        [JsonPropertyName("token")] public string Token { get; } = token;
    }
}
