using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.Friend;

[ApiHandler("set_friend_category")]
public sealed class SetFriendCategoryHandler(BotContext lagrange) : INoResultApiHandler<SetFriendCategoryHandler.Request>
{
    private readonly BotContext _lagrange = lagrange;

    public async ValueTask<MilkyApiResponse> HandleAsync(Request request, CancellationToken ct)
    {
        await _lagrange.SetFriendCategory(request.UserId, request.CategoryId).WaitAsync(ct);
        return new MilkyApiResponse();
    }

    public sealed class Request(long userId, int categoryId)
    {
        [JsonPropertyName("user_id")] public required long UserId { get; init; } = userId;
        [JsonPropertyName("category_id")] public required int CategoryId { get; init; } = categoryId;
    }
}
