using FastEndpoints;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.System;

public sealed class SetNicknameHandler(BotContext lagrange) : Endpoint<SetNicknameHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/set_nickname");
    }
    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
    {
        await lagrange.SetProfile(request.Nickname, null, null, ct);
        return new();
    }
    public sealed class Request(string nickname)
    {
        [JsonPropertyName("nickname")] public required string Nickname { get; init; } = nickname;
    }
}
