using FastEndpoints;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.System;

public sealed class SendProfileLikeHandler(BotContext lagrange) : Endpoint<SendProfileLikeHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/send_profile_like");
    }
    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
    {
        await lagrange.SendLike(request.UserId, request.Count, ct);
        return new();
    }
    public sealed class Request(long userId, uint count = 1)
    {
        [JsonPropertyName("user_id")] public long UserId { get; init; } = userId;
        [JsonPropertyName("count")] public uint Count { get; init; } = count;
    }
}
