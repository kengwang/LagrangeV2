using Lagrange.Core.Common;
using System.Text;
using System.Text.Json;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Http;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.Http;

[HttpServiceAttribute("group.get_announcements", "POST", "https://web.qun.qq.com/cgi-bin/announce/list_announce", "qun.qq.com", "web.qun.qq.com", AuthInjection = HttpAuthInjection.UrlBknFromPSkey | HttpAuthInjection.FormBknFromSkey, UrlTokenName = "bkn")]
[EventSubscribe<GetGroupAnnouncementsEventReq>(Protocols.All)]
internal sealed class GetGroupAnnouncementsHttpService : HttpService<GetGroupAnnouncementsEventReq, GetGroupAnnouncementsEventResp>
{
    protected override async Task<HttpRequestMessage> BuildRequestAsync(BotContext context, GetGroupAnnouncementsEventReq request, CancellationToken cancellationToken)
    {
        if (request.GroupUin <= 0 || request.Count is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(request));
        return await CreateFormRequestAsync(context, HttpMethod.Post, new Uri("https://web.qun.qq.com/cgi-bin/announce/list_announce"), new Dictionary<string, string?>
        {
            ["qid"] = request.GroupUin.ToString(), ["ft"] = "23", ["s"] = request.Start.ToString(),
            ["n"] = request.Count.ToString(), ["i"] = "1", ["ni"] = "1"
        }, cancellationToken);
    }
    protected override Task<GetGroupAnnouncementsEventResp> ParseResponseAsync(BotContext context, GetGroupAnnouncementsEventReq request, HttpResponseMessage response, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken)
    {
        using var document = HttpResponseParser.ParseJson(payload, "group.get_announcements"); var root = document.RootElement; var code = root.TryGetProperty("ec", out var ec) && ec.TryGetInt32(out var parsed) ? parsed : -1; if (code != 0) throw new HttpServiceException("group.get_announcements", root.TryGetProperty("em", out var message) ? message.ToString() : "Group announcement request failed.", businessCode: code);
        var result = new List<BotGroupAnnouncement>(); if (root.TryGetProperty("feeds", out var feeds)) { var values = feeds.ValueKind == JsonValueKind.Array ? feeds.EnumerateArray() : feeds.EnumerateObject().Select(x => x.Value); foreach (var item in values) { var message = item.TryGetProperty("msg", out var msg) && msg.ValueKind == JsonValueKind.Object ? msg : default; result.Add(new BotGroupAnnouncement { Id = Text(item, "fid"), PublisherId = Number(item, "u"), PublishTime = Number(item, "pubt"), Text = message.ValueKind == JsonValueKind.Object ? Text(message, "text") : string.Empty, Pinned = Number(item, "pinned") != 0, ReadCount = (int)Number(item, "read_num") }); } }
        return Task.FromResult(new GetGroupAnnouncementsEventResp(new BotGroupAnnouncementResult { Announcements = result }));
    }
    private static string Text(JsonElement value, string name) => value.TryGetProperty(name, out var item) ? item.ToString() : string.Empty;
    private static long Number(JsonElement value, string name) => value.TryGetProperty(name, out var item) && item.TryGetInt64(out var number) ? number : 0;
}

[HttpServiceAttribute("group.delete_announcement", "POST", "https://web.qun.qq.com/cgi-bin/announce/del_feed", "qun.qq.com", "web.qun.qq.com", AuthInjection = HttpAuthInjection.UrlBknFromPSkey | HttpAuthInjection.FormBknFromSkey, UrlTokenName = "bkn")]
[EventSubscribe<DeleteGroupAnnouncementEventReq>(Protocols.All)]
internal sealed class DeleteGroupAnnouncementHttpService : HttpService<DeleteGroupAnnouncementEventReq, DeleteGroupAnnouncementEventResp>
{
    protected override async Task<HttpRequestMessage> BuildRequestAsync(BotContext context, DeleteGroupAnnouncementEventReq request, CancellationToken cancellationToken)
    {
        if (request.GroupUin <= 0 || string.IsNullOrWhiteSpace(request.AnnouncementId)) throw new ArgumentException("Group and announcement id are required.");
        return await CreateFormRequestAsync(context, HttpMethod.Post, new Uri("https://web.qun.qq.com/cgi-bin/announce/del_feed"), new Dictionary<string, string?>
        {
            ["fid"] = request.AnnouncementId, ["qid"] = request.GroupUin.ToString()
        }, cancellationToken);
    }
    protected override Task<DeleteGroupAnnouncementEventResp> ParseResponseAsync(BotContext context, DeleteGroupAnnouncementEventReq request, HttpResponseMessage response, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken)
    {
        using var document = HttpResponseParser.ParseJson(payload, "group.delete_announcement"); var root = document.RootElement; var code = root.TryGetProperty("ec", out var ec) && ec.TryGetInt32(out var parsed) ? parsed : -1; if (code != 0) throw new HttpServiceException("group.delete_announcement", root.TryGetProperty("em", out var message) ? message.ToString() : "Group announcement deletion failed.", businessCode: code); return Task.FromResult(DeleteGroupAnnouncementEventResp.Instance);
    }
}
