using System.Threading;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.System;

[ApiHandler("set_bio")]
public sealed class SetBioHandler(BotContext lagrange) : INoResultApiHandler<SetBioHandler.Request>
{
    public async ValueTask<MilkyApiResponse> HandleAsync(Request request, CancellationToken ct)
    {
        await lagrange.SetProfile(null, request.Bio, null, ct);
        return new();
    }
    public sealed class Request(string bio)
    {
        [JsonPropertyName("bio")] public required string Bio { get; init; } = bio;
    }
}
