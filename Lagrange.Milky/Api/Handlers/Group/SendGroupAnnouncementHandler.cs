using System.Threading;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.Group;

[ApiHandler("send_group_announcement")]
public sealed class SendGroupAnnouncementHandler(BotContext lagrange) : INoResultApiHandler<SendGroupAnnouncementHandler.Request>
{
    public async ValueTask<MilkyApiResponse> HandleAsync(Request request, CancellationToken ct)
    {
        await lagrange.PublishGroupAnnouncement(request.GroupId, request.Content, new Lagrange.Core.Common.Response.BotGroupAnnouncementOptions
        {
            Pinned = request.Pinned, SendToNewMembers = request.SendToNewMembers,
            ShowEditCard = request.ShowEditCard, ShowPopup = request.ShowPopup,
            ConfirmRequired = request.ConfirmRequired, PictureId = request.PictureId,
            ImageWidth = request.ImageWidth, ImageHeight = request.ImageHeight
        }, ct).WaitAsync(ct);
        return new();
    }
    public sealed class Request(long groupId, string content, bool pinned = false)
    {
        [JsonPropertyName("group_id")] public long GroupId { get; init; } = groupId;
        [JsonPropertyName("content")] public string Content { get; init; } = content;
        [JsonPropertyName("pinned")] public bool Pinned { get; init; } = pinned;
        [JsonPropertyName("send_to_new_members")] public bool SendToNewMembers { get; init; }
        [JsonPropertyName("show_edit_card")] public bool ShowEditCard { get; init; } = true;
        [JsonPropertyName("show_popup")] public bool ShowPopup { get; init; }
        [JsonPropertyName("confirm_required")] public bool ConfirmRequired { get; init; } = true;
        [JsonPropertyName("picture_id")] public string? PictureId { get; init; }
        [JsonPropertyName("image_width")] public int ImageWidth { get; init; } = 540;
        [JsonPropertyName("image_height")] public int ImageHeight { get; init; } = 300;
    }
}
