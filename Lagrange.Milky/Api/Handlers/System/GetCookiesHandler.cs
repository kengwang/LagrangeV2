using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.System;

[ApiHandler("get_cookies")]
public sealed class GetCookiesHandler(BotContext lagrange) : IApiHandler<GetCookiesHandler.Request, GetCookiesHandler.Result>
{
    private readonly BotContext _lagrange = lagrange;

    public async ValueTask<MilkyApiResponse<Result>> HandleAsync(Request request, CancellationToken ct)
    {
        var cookies = await _lagrange.FetchCookies([.. request.Domains]).WaitAsync(ct);
        return new MilkyApiResponse<Result>(new Result { Cookies = cookies });
    }

    public sealed class Request(IReadOnlyList<string>? domains = null)
    {
        [JsonPropertyName("domains")] public IReadOnlyList<string> Domains { get; init; } = domains ?? ["qq.com"];
    }

    public sealed class Result
    {
        [JsonPropertyName("cookies")] public required IReadOnlyDictionary<string, string> Cookies { get; init; }
    }
}
