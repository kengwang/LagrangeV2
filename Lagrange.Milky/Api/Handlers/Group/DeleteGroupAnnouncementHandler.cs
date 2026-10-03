using System.Threading;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.Group;

[ApiHandler("delete_group_announcement")]
public sealed class DeleteGroupAnnouncementHandler(BotContext lagrange) : INoResultApiHandler<DeleteGroupAnnouncementHandler.Request>
{
    public async ValueTask<MilkyApiResponse> HandleAsync(Request request, CancellationToken ct)
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
