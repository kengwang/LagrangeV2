using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.Interaction;

[ApiHandler("send_like")]
public sealed class SendLikeHandler(BotContext lagrange) : INoResultApiHandler<SendLikeHandler.Request>
{
    private readonly BotContext _lagrange = lagrange;

    public async ValueTask<MilkyApiResponse> HandleAsync(Request request, CancellationToken ct)
    {
        await _lagrange.SendLike(request.UserId, checked((uint)request.Count), ct);
        return new MilkyApiResponse();
    }

    public sealed class Request(long userId, int count = 1)
    {
        [JsonPropertyName("user_id")] public required long UserId { get; init; } = userId;
        [JsonPropertyName("count")] public int Count { get; init; } = count;
    }
}
