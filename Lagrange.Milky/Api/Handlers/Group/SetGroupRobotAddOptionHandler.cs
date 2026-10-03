using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;
namespace Lagrange.Milky.Api.Handlers.Group;
[ApiHandler("set_group_robot_add_option")]
public sealed class SetGroupRobotAddOptionHandler(BotContext lagrange) : INoResultApiHandler<SetGroupRobotAddOptionHandler.Request>
{
    public async ValueTask<MilkyApiResponse> HandleAsync(Request request, CancellationToken ct) { await lagrange.SetGroupRobotAddOption(request.GroupId, request.MemberSwitch, request.MemberExamine, ct).WaitAsync(ct); return new MilkyApiResponse(); }
    public sealed class Request { [JsonPropertyName("group_id")] public long GroupId { get; init; } [JsonPropertyName("robot_member_switch")] public uint? MemberSwitch { get; init; } [JsonPropertyName("robot_member_examine")] public uint? MemberExamine { get; init; } }
}
