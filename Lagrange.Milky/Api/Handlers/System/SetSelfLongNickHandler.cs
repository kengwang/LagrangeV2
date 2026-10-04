using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.System;

public sealed class SetSelfLongNickHandler(BotContext lagrange) : Endpoint<SetSelfLongNickHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/set_self_long_nick");
    }
    private readonly BotContext _lagrange = lagrange;

    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
    {
        await _lagrange.SetSelfLongNick(request.LongNick, ct);
        return new MilkyApiResponse();
    }

    public sealed class Request(string longNick)
    {
        [JsonPropertyName("long_nick")] public required string LongNick { get; init; } = longNick;
    }
}
