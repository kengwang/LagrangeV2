using Lagrange.Core.Common;
using System.Text;
using System.Text.Json;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Http;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.Http;

[HttpServiceAttribute("group.get_announcements", "POST", "/cgi-bin/announce/list_announce", "qun.qq.com", "web.qun.qq.com")]
[EventSubscribe<GetGroupAnnouncementsEventReq>(Protocols.All)]
internal sealed class GetGroupAnnouncementsHttpService : HttpService<GetGroupAnnouncementsEventReq, GetGroupAnnouncementsEventResp>
{
    public GetGroupAnnouncementsHttpService() : base("qun.qq.com", "web.qun.qq.com") { }
    protected override async Task<HttpRequestMessage> BuildRequestAsync(BotContext context, GetGroupAnnouncementsEventReq request, CancellationToken cancellationToken)
    {
        if (request.GroupUin <= 0 || request.Count is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(request));
        var pskey = await context.HttpSessionContext.GetPSkeyAsync("qun.qq.com", cancellationToken);
        var bkn = ComputeBkn(pskey); var body = $"qid={request.GroupUin}&bkn={bkn}&ft=23&s={request.Start}&n={request.Count}&i=1&ni=1";
        return new HttpRequestMessage(HttpMethod.Post, $"https://web.qun.qq.com/cgi-bin/announce/list_announce?bkn={bkn}") { Content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded") };
    }
    protected override Task<GetGroupAnnouncementsEventResp> ParseResponseAsync(BotContext context, GetGroupAnnouncementsEventReq request, HttpResponseMessage response, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken)
    {
        using var document = HttpResponseParser.ParseJson(payload, "group.get_announcements"); var root = document.RootElement; var code = root.TryGetProperty("ec", out var ec) && ec.TryGetInt32(out var parsed) ? parsed : -1; if (code != 0) throw new HttpServiceException("group.get_announcements", root.TryGetProperty("em", out var message) ? message.ToString() : "Group announcement request failed.", businessCode: code);
        var result = new List<BotGroupAnnouncement>(); if (root.TryGetProperty("feeds", out var feeds)) { var values = feeds.ValueKind == JsonValueKind.Array ? feeds.EnumerateArray() : feeds.EnumerateObject().Select(x => x.Value); foreach (var item in values) { var message = item.TryGetProperty("msg", out var msg) && msg.ValueKind == JsonValueKind.Object ? msg : default; result.Add(new BotGroupAnnouncement { Id = Text(item, "fid"), PublisherId = Number(item, "u"), PublishTime = Number(item, "pubt"), Text = message.ValueKind == JsonValueKind.Object ? Text(message, "text") : string.Empty, Pinned = Number(item, "pinned") != 0, ReadCount = (int)Number(item, "read_num") }); } }
        return Task.FromResult(new GetGroupAnnouncementsEventResp(new BotGroupAnnouncementResult { Announcements = result }));
    }
    private static string Text(JsonElement value, string name) => value.TryGetProperty(name, out var item) ? item.ToString() : string.Empty;
    private static long Number(JsonElement value, string name) => value.TryGetProperty(name, out var item) && item.TryGetInt64(out var number) ? number : 0;
    private static string ComputeBkn(string skey) { uint hash = 5381; foreach (var c in skey) hash += (hash << 5) + c; return (hash & 0x7FFFFFFF).ToString(); }
}

[HttpServiceAttribute("group.delete_announcement", "POST", "/cgi-bin/announce/del_feed", "qun.qq.com", "web.qun.qq.com")]
[EventSubscribe<DeleteGroupAnnouncementEventReq>(Protocols.All)]
internal sealed class DeleteGroupAnnouncementHttpService : HttpService<DeleteGroupAnnouncementEventReq, DeleteGroupAnnouncementEventResp>
{
    public DeleteGroupAnnouncementHttpService() : base("qun.qq.com", "web.qun.qq.com") { }
    protected override async Task<HttpRequestMessage> BuildRequestAsync(BotContext context, DeleteGroupAnnouncementEventReq request, CancellationToken cancellationToken)
    {
        if (request.GroupUin <= 0 || string.IsNullOrWhiteSpace(request.AnnouncementId)) throw new ArgumentException("Group and announcement id are required.");
        var pskey = await context.HttpSessionContext.GetPSkeyAsync("qun.qq.com", cancellationToken); var bkn = ComputeBkn(pskey); var body = $"bkn={bkn}&fid={Uri.EscapeDataString(request.AnnouncementId)}&qid={request.GroupUin}";
        return new HttpRequestMessage(HttpMethod.Post, $"https://web.qun.qq.com/cgi-bin/announce/del_feed?bkn={bkn}") { Content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded") };
    }
    protected override Task<DeleteGroupAnnouncementEventResp> ParseResponseAsync(BotContext context, DeleteGroupAnnouncementEventReq request, HttpResponseMessage response, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken)
    {
        using var document = HttpResponseParser.ParseJson(payload, "group.delete_announcement"); var root = document.RootElement; var code = root.TryGetProperty("ec", out var ec) && ec.TryGetInt32(out var parsed) ? parsed : -1; if (code != 0) throw new HttpServiceException("group.delete_announcement", root.TryGetProperty("em", out var message) ? message.ToString() : "Group announcement deletion failed.", businessCode: code); return Task.FromResult(DeleteGroupAnnouncementEventResp.Instance);
    }
    private static string ComputeBkn(string skey) { uint hash = 5381; foreach (var c in skey) hash += (hash << 5) + c; return (hash & 0x7FFFFFFF).ToString(); }
}
