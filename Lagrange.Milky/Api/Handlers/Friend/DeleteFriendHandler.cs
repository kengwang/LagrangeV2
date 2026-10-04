using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.Friend;

public sealed class DeleteFriendHandler(BotContext lagrange) : Endpoint<DeleteFriendHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/delete_friend");
    }
    private readonly BotContext _lagrange = lagrange;

    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
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
