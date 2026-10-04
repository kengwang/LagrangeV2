using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.Group;

public sealed class KickGroupMemberHandler(BotContext lagrange) : Endpoint<KickGroupMemberHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/kick_group_member");
    }
    private readonly BotContext _lagrange = lagrange;

    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
    {
        return await _lagrange.KickGroupMember(request.GroupId, request.UserId, request.RejectAddRequest).WaitAsync(ct)
            ? new MilkyApiResponse(-500, "unknown error")
            : new MilkyApiResponse();
    }

    public sealed class Request(long groupId, long userId, bool rejectAddRequest = false)
    {
        [JsonPropertyName("group_id")] public required long GroupId { get; init; } = groupId;
        [JsonPropertyName("user_id")] public required long UserId { get; init; } = userId;
        [JsonPropertyName("reject_add_request")] public bool RejectAddRequest { get; init; } = rejectAddRequest;
    }
}
