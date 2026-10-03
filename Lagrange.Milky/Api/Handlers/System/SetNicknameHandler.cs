using System.Threading;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.System;

[ApiHandler("set_nickname")]
public sealed class SetNicknameHandler(BotContext lagrange) : INoResultApiHandler<SetNicknameHandler.Request>
{
    public async ValueTask<MilkyApiResponse> HandleAsync(Request request, CancellationToken ct)
    {
        await lagrange.SetProfile(request.Nickname, null, null, ct);
        return new();
    }
    public sealed class Request(string nickname)
    {
        [JsonPropertyName("nickname")] public required string Nickname { get; init; } = nickname;
    }
}
