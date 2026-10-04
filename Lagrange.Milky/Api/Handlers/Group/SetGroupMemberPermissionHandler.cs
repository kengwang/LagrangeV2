using FastEndpoints;
using System;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.Group;

public sealed class SetGroupMemberPermissionHandler(BotContext lagrange) : Endpoint<SetGroupMemberPermissionHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/set_group_member_permission");
    }
    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Permission);
        await lagrange.SetGroupMemberPermission(request.GroupId, request.Permission, request.Allow, request.CurrentPrivilegeFlag, ct).WaitAsync(ct);
        return new MilkyApiResponse();
    }
    public sealed class Request
    {
        [JsonPropertyName("group_id")] public required long GroupId { get; init; }
        [JsonPropertyName("permission")] public required string Permission { get; init; }
        [JsonPropertyName("allow")] public bool Allow { get; init; }
        [JsonPropertyName("current_privilege_flag")] public uint CurrentPrivilegeFlag { get; init; }
    }
}
