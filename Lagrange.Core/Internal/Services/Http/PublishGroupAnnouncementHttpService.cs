using Lagrange.Core.Common;
using System.Text;
using System.Text.Json;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Http;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.Http;

[HttpServiceAttribute("group.publish_announcement", "POST", "/cgi-bin/announce/add_qun_notice", "qun.qq.com", "web.qun.qq.com", AuthInjection = HttpAuthInjection.UrlBknFromPSkey | HttpAuthInjection.FormBknFromSkey, UrlTokenName = "bkn")]
[EventSubscribe<PublishGroupAnnouncementEventReq>(Protocols.All)]
internal sealed class PublishGroupAnnouncementHttpService : HttpService<PublishGroupAnnouncementEventReq, PublishGroupAnnouncementEventResp>
{
    protected override async Task<HttpRequestMessage> BuildRequestAsync(BotContext context, PublishGroupAnnouncementEventReq request, CancellationToken cancellationToken)
    {
        if (request.GroupUin <= 0 || string.IsNullOrWhiteSpace(request.Content)) throw new ArgumentException("Group and announcement content are required.");
        ArgumentNullException.ThrowIfNull(request.Options);
        var o = request.Options;
        if (o.PictureId is not null && (o.ImageWidth <= 0 || o.ImageHeight <= 0)) throw new ArgumentOutOfRangeException(nameof(request.Options));
        var settings = $"{{\"is_show_edit_card\":{(o.ShowEditCard ? 1 : 0)},\"tip_window_type\":{(o.ShowPopup ? 0 : 1)},\"confirm_required\":{(o.ConfirmRequired ? 1 : 0)}}}";
        var fields = new Dictionary<string, string> { ["qid"] = request.GroupUin.ToString(), ["text"] = request.Content, ["pinned"] = o.Pinned ? "1" : "0", ["type"] = o.SendToNewMembers ? "20" : "1", ["settings"] = settings };
        if (!string.IsNullOrWhiteSpace(o.PictureId)) { fields["pic"] = o.PictureId; fields["imgWidth"] = o.ImageWidth.ToString(); fields["imgHeight"] = o.ImageHeight.ToString(); }
        var endpoint = o.SendToNewMembers ? "add_qun_instruction" : "add_qun_notice";
        return await CreateFormRequestAsync(context, HttpMethod.Post, new Uri($"https://web.qun.qq.com/cgi-bin/announce/{endpoint}"), fields.Select(pair => new KeyValuePair<string, string?>(pair.Key, pair.Value)), cancellationToken);
    }
    protected override Task<PublishGroupAnnouncementEventResp> ParseResponseAsync(BotContext context, PublishGroupAnnouncementEventReq request, HttpResponseMessage response, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken)
    {
        using var document = HttpResponseParser.ParseJson(payload, "group.publish_announcement"); var root = document.RootElement; var code = root.TryGetProperty("ec", out var ec) && ec.TryGetInt32(out var parsed) ? parsed : -1; if (code != 0) throw new HttpServiceException("group.publish_announcement", root.TryGetProperty("em", out var message) ? message.ToString() : "Group announcement publish failed.", businessCode: code); return Task.FromResult(PublishGroupAnnouncementEventResp.Instance);
    }
}
