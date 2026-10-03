using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.Friend;

[ApiHandler("delete_friend")]
public sealed class DeleteFriendHandler(BotContext lagrange) : INoResultApiHandler<DeleteFriendHandler.Request>
{
    private readonly BotContext _lagrange = lagrange;

    public async ValueTask<MilkyApiResponse> HandleAsync(Request request, CancellationToken ct)
    {
        await _lagrange.DeleteFriend(request.UserId, request.Block).WaitAsync(ct);
        return new MilkyApiResponse();
    }

    public sealed class Request(long userId, bool block = false)
    {
        [JsonPropertyName("user_id")] public required long UserId { get; init; } = userId;
        [JsonPropertyName("block")] public bool Block { get; init; } = block;
    }
}
