using Lagrange.Core.Common;
using System.Text.Json;
using System.Text;
using System.Text.RegularExpressions;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Http;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.Http;

[HttpServiceAttribute("qzone.friend_dress", "GET", "/v2/pages/aioDressPage", "qzone.qq.com")]
[EventSubscribe<GetFriendDressEventReq>(Protocols.All)]
internal sealed class FriendDressHttpService : HttpService<GetFriendDressEventReq, GetFriendDressEventResp>
{
    public FriendDressHttpService() : base("qzone.qq.com") { }
    protected override Task<HttpRequestMessage> BuildRequestAsync(BotContext context, GetFriendDressEventReq request, CancellationToken cancellationToken)
    {
        if (request.TargetUin <= 0) throw new ArgumentOutOfRangeException(nameof(request.TargetUin));
        const string trace = "base64-eyJhcHBpZCI6InRvYWlvIiwicGFnZV9pZCI6IjM3IiwiaXRlbV9pZCI6IiIsIml0ZW1fdHlwZSI6IjIifQ%3D%3D";
        var inner = $"https://zb.vip.qq.com/v2/pages/aioDressPage?fromPage=1&targetUin={request.TargetUin}&widgetId=0&fontEffectId=0&bgId=custom&chatId={request.TargetUin}&isGroup=0&traceDetail={trace}";
        var url = $"https://zb.vip.qq.com/v2/pages/aioDressPage?fromPage=1&enteranceId=aio&url={Uri.EscapeDataString(inner)}&fontEffectId=0&chatId={request.TargetUin}&widgetId=0&targetUin={request.TargetUin}&isGroup=0&bgId=custom&traceDetail={trace}";
        return CreateGetRequestAsync(context, new Uri(url), cancellationToken);
    }
    protected override Task<GetFriendDressEventResp> ParseResponseAsync(BotContext context, GetFriendDressEventReq request, HttpResponseMessage response, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken)
    {
        var html = Encoding.UTF8.GetString(payload.Span);
        var match = Regex.Match(html, "window\\.__INITIAL_ASYNCDATA__\\s*=\\s*(\\{[\\s\\S]*?\\});\\(function", RegexOptions.CultureInvariant);
        if (!match.Success) throw new HttpServiceException("qzone.friend_dress", "Friend dress page data is unavailable.");
        using var document = JsonDocument.Parse(match.Groups[1].Value); var root = document.RootElement;
        var returned = Text(root, "targetUin"); if (returned != request.TargetUin.ToString()) throw new HttpServiceException("qzone.friend_dress", "Friend dress target UIN mismatch.");
        var items = new List<BotFriendDressItem>();
        if (root.TryGetProperty("rawUsingList", out var list) && list.ValueKind == JsonValueKind.Array)
            foreach (var item in list.EnumerateArray()) { var app = Number(item, "appId"); if (app is 2 or 5 or 23) continue; items.Add(new BotFriendDressItem { AppId = app, Kind = app switch { 4 => "挂件", 15 => "名片", 17 => "来电", 22 => "彩色屏保", 47 => "头像双击动作", 352 => "输入状态", _ => $"appId={app}" }, ItemId = Number(item, "itemId"), Name = Text(item, "name"), PreviewUrl = Text(item, "image"), Price = item.TryGetProperty("extrainfo", out var extra) ? Number(extra, "price") : 0 }); }
        return Task.FromResult(new GetFriendDressEventResp(new BotFriendDressResult { TargetUin = returned, IsSvip = root.TryGetProperty("isSvip", out var svip) && svip.ValueKind == JsonValueKind.True, AvatarUrl = Text(root, "avatarImage"), Items = items }));
    }
    private static string Text(JsonElement value, string name) => value.TryGetProperty(name, out var item) ? item.ToString() : string.Empty;
    private static int Number(JsonElement value, string name) => value.TryGetProperty(name, out var item) && item.TryGetInt32(out var number) ? number : 0;
}
