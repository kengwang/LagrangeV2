using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.Friend;

public sealed class HandleFriendRequestHandler(BotContext lagrange) : Endpoint<HandleFriendRequestHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/handle_friend_request");
    }
    private readonly BotContext _lagrange = lagrange;

    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
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
