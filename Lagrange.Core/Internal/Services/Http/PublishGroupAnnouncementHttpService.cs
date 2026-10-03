using Lagrange.Core.Common;
using System.Text;
using System.Text.Json;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Http;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.Http;

[HttpServiceAttribute("group.publish_announcement", "POST", "/cgi-bin/announce/add_qun_notice", "qun.qq.com", "web.qun.qq.com")]
[EventSubscribe<PublishGroupAnnouncementEventReq>(Protocols.All)]
internal sealed class PublishGroupAnnouncementHttpService : HttpService<PublishGroupAnnouncementEventReq, PublishGroupAnnouncementEventResp>
{
    public PublishGroupAnnouncementHttpService() : base("qun.qq.com", "web.qun.qq.com") { }
    protected override async Task<HttpRequestMessage> BuildRequestAsync(BotContext context, PublishGroupAnnouncementEventReq request, CancellationToken cancellationToken)
    {
        if (request.GroupUin <= 0 || string.IsNullOrWhiteSpace(request.Content)) throw new ArgumentException("Group and announcement content are required.");
        var pskey = await context.HttpSessionContext.GetPSkeyAsync("qun.qq.com", cancellationToken); var skey = context.Keystore.WLoginSigs.SKey is { Length: > 0 } value ? Encoding.UTF8.GetString(value) : string.Empty; if (string.IsNullOrWhiteSpace(skey)) throw new HttpServiceException("group.publish_announcement", "Group web skey is unavailable.");
        var bkn = ComputeBkn(skey); var urlToken = ComputeBkn(pskey); var o = request.Options; if (o.ImageWidth <= 0 || o.ImageHeight <= 0) throw new ArgumentOutOfRangeException(nameof(request.Options));
        var settings = $"{{\"is_show_edit_card\":{(o.ShowEditCard ? 1 : 0)},\"tip_window_type\":{(o.ShowPopup ? 0 : 1)},\"confirm_required\":{(o.ConfirmRequired ? 1 : 0)}}}";
        var fields = new Dictionary<string, string> { ["qid"] = request.GroupUin.ToString(), ["bkn"] = bkn, ["text"] = request.Content, ["pinned"] = o.Pinned ? "1" : "0", ["type"] = o.SendToNewMembers ? "20" : "1", ["settings"] = settings };
        if (!string.IsNullOrWhiteSpace(o.PictureId)) { fields["pic"] = o.PictureId; fields["imgWidth"] = o.ImageWidth.ToString(); fields["imgHeight"] = o.ImageHeight.ToString(); }
        var endpoint = o.SendToNewMembers ? "add_qun_instruction" : "add_qun_notice";
        return new HttpRequestMessage(HttpMethod.Post, $"https://web.qun.qq.com/cgi-bin/announce/{endpoint}?bkn={urlToken}") { Content = new FormUrlEncodedContent(fields) };
    }
    protected override Task<PublishGroupAnnouncementEventResp> ParseResponseAsync(BotContext context, PublishGroupAnnouncementEventReq request, HttpResponseMessage response, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken)
    {
        using var document = HttpResponseParser.ParseJson(payload, "group.publish_announcement"); var root = document.RootElement; var code = root.TryGetProperty("ec", out var ec) && ec.TryGetInt32(out var parsed) ? parsed : -1; if (code != 0) throw new HttpServiceException("group.publish_announcement", root.TryGetProperty("em", out var message) ? message.ToString() : "Group announcement publish failed.", businessCode: code); return Task.FromResult(PublishGroupAnnouncementEventResp.Instance);
    }
    private static string ComputeBkn(string skey) { uint hash = 5381; foreach (var c in skey) hash += (hash << 5) + c; return (hash & 0x7FFFFFFF).ToString(); }
}
