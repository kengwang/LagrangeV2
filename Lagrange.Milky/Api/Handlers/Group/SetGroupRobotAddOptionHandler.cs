using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
namespace Lagrange.Milky.Api.Handlers.Group;
public sealed class SetGroupRobotAddOptionHandler(BotContext lagrange) : Endpoint<SetGroupRobotAddOptionHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/set_group_robot_add_option");
    }
    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct) { await lagrange.SetGroupRobotAddOption(request.GroupId, request.MemberSwitch, request.MemberExamine, ct).WaitAsync(ct); return new MilkyApiResponse(); }
    public sealed class Request { [JsonPropertyName("group_id")] public long GroupId { get; init; } [JsonPropertyName("robot_member_switch")] public uint? MemberSwitch { get; init; } [JsonPropertyName("robot_member_examine")] public uint? MemberExamine { get; init; } }
}
