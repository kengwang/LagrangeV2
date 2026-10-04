using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;

namespace Lagrange.Milky.Api.Handlers.System;

public sealed class GetLoginInfoHandler(BotContext lagrange) : EndpointWithoutRequest<MilkyApiResponse<GetLoginInfoHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/get_login_info");
    }
    private readonly BotContext _lagrange = lagrange;

    public override Task<MilkyApiResponse<Result>> ExecuteAsync(CancellationToken ct)
    {
        return Task.FromResult(new MilkyApiResponse<Result>(new Result
        {
            Uin = _lagrange.BotUin,
            Nickname = _lagrange.BotInfo?.Name ?? string.Empty,
        }));
    }

    public sealed class Result
    {
        [JsonPropertyName("uin")] public required long Uin { get; init; }
        [JsonPropertyName("nickname")] public required string Nickname { get; init; }
    }
}
