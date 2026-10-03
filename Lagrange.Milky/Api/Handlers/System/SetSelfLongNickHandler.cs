using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.System;

[ApiHandler("set_self_long_nick")]
public sealed class SetSelfLongNickHandler(BotContext lagrange) : INoResultApiHandler<SetSelfLongNickHandler.Request>
{
    private readonly BotContext _lagrange = lagrange;

    public async ValueTask<MilkyApiResponse> HandleAsync(Request request, CancellationToken ct)
    {
        await _lagrange.SetSelfLongNick(request.LongNick, ct);
        return new MilkyApiResponse();
    }

    public sealed class Request(string longNick)
    {
        [JsonPropertyName("long_nick")] public required string LongNick { get; init; } = longNick;
    }
}
