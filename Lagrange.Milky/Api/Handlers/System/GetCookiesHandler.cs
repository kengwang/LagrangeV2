using FastEndpoints;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.System;

public sealed class GetCookiesHandler(BotContext lagrange) : Endpoint<GetCookiesHandler.Request, MilkyApiResponse<GetCookiesHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/get_cookies");
    }
    private readonly BotContext _lagrange = lagrange;

    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
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
