using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.Friend;

public sealed class SetFriendRemarkHandler(BotContext lagrange) : Endpoint<SetFriendRemarkHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/set_friend_remark");
    }
    private readonly BotContext _lagrange = lagrange;

    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
    {
        await _lagrange.SetFriendRemark(request.UserId, request.Remark).WaitAsync(ct);
        return new MilkyApiResponse();
    }

    public sealed class Request(long userId, string remark)
    {
        [JsonPropertyName("user_id")] public required long UserId { get; init; } = userId;
        [JsonPropertyName("remark")] public required string Remark { get; init; } = remark;
    }
}
