using FastEndpoints;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.Group;

public sealed class DeleteGroupAnnouncementHandler(BotContext lagrange) : Endpoint<DeleteGroupAnnouncementHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/delete_group_announcement");
    }
    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
    {
        await lagrange.DeleteGroupAnnouncement(request.GroupId, request.AnnouncementId, ct).WaitAsync(ct);
        return new();
    }
    public sealed class Request(long groupId, string announcementId)
    {
        [JsonPropertyName("group_id")] public long GroupId { get; init; } = groupId;
        [JsonPropertyName("announcement_id")] public string AnnouncementId { get; init; } = announcementId;
    }
}
