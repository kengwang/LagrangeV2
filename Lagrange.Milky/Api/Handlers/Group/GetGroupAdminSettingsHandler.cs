using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
namespace Lagrange.Milky.Api.Handlers.Group;
public sealed class GetGroupAdminSettingsHandler(BotContext lagrange) : Endpoint<GetGroupAdminSettingsHandler.Request, MilkyApiResponse<GetGroupAdminSettingsHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/get_group_admin_settings");
    }
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct) { var x = await lagrange.GetGroupAdminSettings(request.GroupId, ct).WaitAsync(ct); return new(new Result { AddType = x.AddType, GroupQuestion = x.GroupQuestion, GroupAnswer = x.GroupAnswer, MemberInvitePolicy = x.MemberInvitePolicy, NewMemberHistoryVisible = x.NewMemberHistoryVisible, RobotMemberSwitch = x.RobotMemberSwitch, RobotMemberExamine = x.RobotMemberExamine }); }
    public sealed class Request { [JsonPropertyName("group_id")] public long GroupId { get; init; } }
    public sealed class Result { [JsonPropertyName("add_type")] public uint AddType { get; init; } [JsonPropertyName("group_question")] public string GroupQuestion { get; init; } = string.Empty; [JsonPropertyName("group_answer")] public string GroupAnswer { get; init; } = string.Empty; [JsonPropertyName("member_invite_policy")] public string MemberInvitePolicy { get; init; } = string.Empty; [JsonPropertyName("new_member_history_visible")] public bool NewMemberHistoryVisible { get; init; } [JsonPropertyName("robot_member_switch")] public uint RobotMemberSwitch { get; init; } [JsonPropertyName("robot_member_examine")] public uint RobotMemberExamine { get; init; } }
}
