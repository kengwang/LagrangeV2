using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.Friend;

public sealed class SetFriendCategoryHandler(BotContext lagrange) : Endpoint<SetFriendCategoryHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/set_friend_category");
    }
    private readonly BotContext _lagrange = lagrange;

    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
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
