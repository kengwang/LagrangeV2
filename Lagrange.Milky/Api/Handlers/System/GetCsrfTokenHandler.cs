using FastEndpoints;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.System;

public sealed class GetCsrfTokenHandler(BotContext lagrange) : EndpointWithoutRequest<MilkyApiResponse<GetCsrfTokenHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/get_csrf_token");
    }
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(CancellationToken ct)
        => new(new Result(await lagrange.FetchCsrfToken(ct).WaitAsync(ct)));

    public sealed class Result(string token)
    {
        [JsonPropertyName("token")] public string Token { get; } = token;
    }
}
