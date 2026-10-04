using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Converters;

namespace Lagrange.Milky.Api.Handlers.System;

public sealed class SetAvatarHandler(BotContext lagrange, ResourceConverter resourceConverter) : Endpoint<SetAvatarHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/set_avatar");
    }
    private readonly BotContext _lagrange = lagrange;
    private readonly ResourceConverter _resourceConverter = resourceConverter;

    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
    {
        using var stream = await _resourceConverter.UriToStreamAsync(request.Uri, ct);
        return await _lagrange.SetBotAvatar(stream)
            ? new MilkyApiResponse()
            : new MilkyApiResponse(-500, "unknown error");
    }

    public sealed class Request(string uri)
    {
        [JsonPropertyName("uri")] public required string Uri { get; init; } = uri;
    }
}
