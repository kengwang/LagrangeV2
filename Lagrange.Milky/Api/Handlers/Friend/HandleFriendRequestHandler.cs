using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.Friend;

[ApiHandler("handle_friend_request")]
public sealed class HandleFriendRequestHandler(BotContext lagrange) : INoResultApiHandler<HandleFriendRequestHandler.Request>
{
    private readonly BotContext _lagrange = lagrange;

    public async ValueTask<MilkyApiResponse> HandleAsync(Request request, CancellationToken ct)
    {
        await _lagrange.HandleFriendRequest(request.Flag, request.Approve).WaitAsync(ct);
        return new MilkyApiResponse();
    }

    public sealed class Request(string flag, bool approve)
    {
        [JsonPropertyName("flag")] public required string Flag { get; init; } = flag;
        [JsonPropertyName("approve")] public bool Approve { get; init; } = approve;
    }
}
