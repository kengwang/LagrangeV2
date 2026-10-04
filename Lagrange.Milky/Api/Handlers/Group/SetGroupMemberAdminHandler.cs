using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.Group;

public sealed class SetGroupMemberAdminHandler(BotContext lagrange) : Endpoint<SetGroupMemberAdminHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/set_group_member_admin");
    }
    private readonly BotContext _lagrange = lagrange;

    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
    {
        await _lagrange.SetGroupAdmin(request.GroupId, request.UserId, request.IsSet).WaitAsync(ct);
        return new MilkyApiResponse();
    }

    public sealed class Request(long groupId, long userId, bool isSet)
    {
        [JsonPropertyName("group_id")] public required long GroupId { get; init; } = groupId;
        [JsonPropertyName("user_id")] public required long UserId { get; init; } = userId;
        [JsonPropertyName("is_set")] public bool IsSet { get; init; } = isSet;
    }
}
