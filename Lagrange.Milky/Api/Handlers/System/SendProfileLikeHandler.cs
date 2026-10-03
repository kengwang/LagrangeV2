using System.Threading;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.System;

[ApiHandler("send_profile_like")]
public sealed class SendProfileLikeHandler(BotContext lagrange) : INoResultApiHandler<SendProfileLikeHandler.Request>
{
    public async ValueTask<MilkyApiResponse> HandleAsync(Request request, CancellationToken ct)
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
