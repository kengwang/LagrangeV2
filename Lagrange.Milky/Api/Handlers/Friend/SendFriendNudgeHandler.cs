using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.Friend;

public sealed class SendFriendNudgeHandler(BotContext lagrange) : Endpoint<SendFriendNudgeHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/send_friend_nudge");
    }
    private readonly BotContext _lagrange = lagrange;

    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
    {
        await _lagrange.SendFriendNudge(
            request.UserId,
            request.IsSelf ? _lagrange.BotUin : request.UserId
        ).WaitAsync(ct);
        return new MilkyApiResponse();
    }

    public sealed class Request(long userId, bool isSelf = false)
    {
        [JsonPropertyName("user_id")] public required long UserId { get; init; } = userId;
        [JsonPropertyName("is_self")] public bool IsSelf { get; init; } = isSelf;
    }
}
