using Lagrange.Core.Common;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Http;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.Http;

[HttpServiceAttribute("group.get_honor", "GET", "/interactive/honorlist", "qun.qq.com", AuthInjection = HttpAuthInjection.UrlBknFromPSkey, UrlTokenName = "bkn")]
[EventSubscribe<GetGroupHonorEventReq>(Protocols.All)]
internal sealed class GroupHonorHttpService : HttpService<GetGroupHonorEventReq, GetGroupHonorEventResp>
{
    public GroupHonorHttpService() : base("qun.qq.com") { }
    protected override async Task<HttpRequestMessage> BuildRequestAsync(BotContext context, GetGroupHonorEventReq request, CancellationToken cancellationToken)
    {
        if (request.GroupUin <= 0 || string.IsNullOrWhiteSpace(request.HonorType)) throw new ArgumentException("Group and honor type are required.");
        if (request.HonorType is not ("all" or "talkative" or "performer" or "legend" or "emotion")) throw new ArgumentException("Unknown honor type.");
        var type = request.HonorType == "all" ? 1 : request.HonorType switch { "talkative" => 1, "performer" => 2, "legend" => 3, _ => 6 };
        return await CreateGetRequestAsync(context, new Uri($"https://qun.qq.com/interactive/honorlist?gc={request.GroupUin}&type={type}"), cancellationToken);
    }
    protected override Task<GetGroupHonorEventResp> ParseResponseAsync(BotContext context, GetGroupHonorEventReq request, HttpResponseMessage response, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken)
    {
        var html = Encoding.UTF8.GetString(payload.Span); var match = Regex.Match(html, "window\\.__INITIAL_STATE__\\s*=\\s*(\\{[\\s\\S]*?\\});", RegexOptions.CultureInvariant); if (!match.Success) throw new HttpServiceException("group.get_honor", "Group honor response state is unavailable.");
        using var document = JsonDocument.Parse(match.Groups[1].Value); var root = document.RootElement; var listName = request.HonorType == "talkative" ? "talkativeList" : "actorList"; var items = new List<BotGroupHonorItem>();
        if (root.TryGetProperty(listName, out var list) && list.ValueKind == JsonValueKind.Array) foreach (var item in list.EnumerateArray()) items.Add(new BotGroupHonorItem { UserId = item.TryGetProperty("uin", out var uin) && uin.TryGetInt64(out var id) ? id : null, Nickname = Text(item, "name"), Avatar = Text(item, "avatar"), Description = Text(item, "desc") });
        return Task.FromResult(new GetGroupHonorEventResp(new BotGroupHonorResult { HonorType = request.HonorType, Items = items }));
    }
    private static string Text(JsonElement value, string name) => value.TryGetProperty(name, out var item) ? item.ToString() : string.Empty;
}
