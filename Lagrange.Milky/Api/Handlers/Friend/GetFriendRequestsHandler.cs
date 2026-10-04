using FastEndpoints;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.Friend;

public sealed class GetFriendRequestsHandler(BotContext lagrange) : EndpointWithoutRequest<MilkyApiResponse<GetFriendRequestsHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/get_friend_requests");
    }
    private readonly BotContext _lagrange = lagrange;

    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(CancellationToken ct)
    {
        var requests = await _lagrange.FetchFriendRequests(ct);
        return new MilkyApiResponse<Result>(new Result
        {
            Requests = [.. requests.Select(request => new Request
            {
                TargetUserId = request.TargetUin,
                SourceUserId = request.SourceUin,
                Comment = request.Comment,
                Source = request.Source,
                Time = request.Time,
                State = request.EventState switch
                {
                    Lagrange.Core.Common.Entity.BotFriendRequest.State.Pending => "pending",
                    Lagrange.Core.Common.Entity.BotFriendRequest.State.Approved => "accepted",
                    _ => "rejected",
                },
            })],
        });
    }

    public sealed class Result
    {
        [JsonPropertyName("requests")] public required IReadOnlyList<Request> Requests { get; init; }
    }

    public sealed class Request
    {
        [JsonPropertyName("target_user_id")] public required long TargetUserId { get; init; }
        [JsonPropertyName("source_user_id")] public required long SourceUserId { get; init; }
        [JsonPropertyName("comment")] public required string Comment { get; init; }
        [JsonPropertyName("source")] public required string Source { get; init; }
        [JsonPropertyName("time")] public required long Time { get; init; }
        [JsonPropertyName("state")] public required string State { get; init; }
    }
}
