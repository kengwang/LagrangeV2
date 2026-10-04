using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.System;

public sealed class SetProfileHandler(BotContext lagrange) : Endpoint<SetProfileHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/set_profile");
    }
    private readonly BotContext _lagrange = lagrange;

    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
    {
        await _lagrange.SetProfile(request.Nickname, request.PersonalNote, request.Sex, ct);
        return new MilkyApiResponse();
    }

    public sealed class Request(string? nickname = null, string? personalNote = null, int? sex = null)
    {
        [JsonPropertyName("nickname")] public string? Nickname { get; init; } = nickname;
        [JsonPropertyName("personal_note")] public string? PersonalNote { get; init; } = personalNote;
        [JsonPropertyName("sex")] public int? Sex { get; init; } = sex;
    }
}
