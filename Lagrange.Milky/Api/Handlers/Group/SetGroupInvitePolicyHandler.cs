using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.Group;

public sealed class SetGroupInvitePolicyHandler(BotContext lagrange) : Endpoint<SetGroupInvitePolicyHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/set_group_member_invite_policy");
    }
    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
    {
        await lagrange.SetGroupInvitePolicy(request.GroupId, request.CurrentPrivilegeFlag, request.Policy, ct).WaitAsync(ct);
        return new MilkyApiResponse();
    }
    public sealed class Request
    {
        [JsonPropertyName("group_id")] public long GroupId { get; init; }
        [JsonPropertyName("current_privilege_flag")] public uint CurrentPrivilegeFlag { get; init; }
        [JsonPropertyName("policy")] public required string Policy { get; init; }
    }
}
