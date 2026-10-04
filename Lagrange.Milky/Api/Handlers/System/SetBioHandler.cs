using FastEndpoints;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.System;

public sealed class SetBioHandler(BotContext lagrange) : Endpoint<SetBioHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/set_bio");
    }
    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
    {
        await lagrange.SetProfile(null, request.Bio, null, ct);
        return new();
    }
    public sealed class Request(string bio)
    {
        [JsonPropertyName("bio")] public required string Bio { get; init; } = bio;
    }
}
