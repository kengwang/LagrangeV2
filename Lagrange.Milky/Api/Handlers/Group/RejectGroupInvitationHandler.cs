using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.Group;

public sealed class RejectGroupInvitationHandler(BotContext lagrange) : Endpoint<RejectGroupInvitationHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/reject_group_invitation");
    }
    private readonly BotContext _lagrange = lagrange;

    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
    {
        await _lagrange.SetGroupNotification(
            request.GroupId,
            (ulong)request.InvitationSeq,
            BotGroupNotificationType.Invite,
            false,
            GroupNotificationOperate.Deny
        ).WaitAsync(ct);
        return new MilkyApiResponse();
    }

    public sealed class Request(long groupId, long invitationSeq)
    {
        [JsonPropertyName("group_id")] public required long GroupId { get; init; } = groupId;
        [JsonPropertyName("invitation_seq")] public required long InvitationSeq { get; init; } = invitationSeq;
    }
}
