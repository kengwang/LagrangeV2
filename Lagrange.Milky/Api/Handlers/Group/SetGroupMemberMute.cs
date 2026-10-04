using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.Group;

public class SetGroupMemberMute(BotContext lagrange) : Endpoint<SetGroupMemberMute.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/set_group_member_mute");
    }
    private readonly BotContext _lagrange = lagrange;

    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
    {
        return !await _lagrange.MuteGroupMember(request.GroupId, request.UserId, (uint)request.Duration).WaitAsync(ct)
            ? new MilkyApiResponse(-500, "unknown error")
            : new MilkyApiResponse();
    }

    public sealed class Request(long groupId, long userId, int duration = 0)
    {
        [JsonPropertyName("group_id")] public long GroupId { get; init; } = groupId;
        [JsonPropertyName("user_id")] public long UserId { get; init; } = userId;
        [JsonPropertyName("duration")] public int Duration { get; init; } = duration;
    }
}
