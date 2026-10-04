using Lagrange.Core.Common;
using System.Text.Json;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Http;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.Http;

[HttpServiceAttribute("group.get_essence", "GET", "/cgi-bin/group_digest/digest_list", "qun.qq.com", AuthInjection = HttpAuthInjection.UrlBknFromPSkey, UrlTokenName = "bkn")]
[EventSubscribe<GetGroupEssenceEventReq>(Protocols.All)]
internal sealed class GroupEssenceHttpService : HttpService<GetGroupEssenceEventReq, GetGroupEssenceEventResp>
{
    protected override async Task<HttpRequestMessage> BuildRequestAsync(BotContext context, GetGroupEssenceEventReq request, CancellationToken cancellationToken)
    {
        if (request.GroupUin <= 0 || request.PageStart < 0 || request.PageLimit is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(request));
        var query = $"page_start={request.PageStart}&page_limit={request.PageLimit}&group_code={request.GroupUin}";
        return await CreateGetRequestAsync(context, new Uri($"https://qun.qq.com/cgi-bin/group_digest/digest_list?{query}"), cancellationToken);
    }
    protected override Task<GetGroupEssenceEventResp> ParseResponseAsync(BotContext context, GetGroupEssenceEventReq request, HttpResponseMessage response, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken)
    {
        using var document = HttpResponseParser.ParseJson(payload, "group.get_essence"); var root = document.RootElement;
        var code = root.TryGetProperty("retcode", out var codeElement) && codeElement.TryGetInt32(out var parsed) ? parsed : -1;
        if (code != 0) throw new HttpServiceException("group.get_essence", root.TryGetProperty("retmsg", out var message) ? message.ToString() : "Group essence request failed.", businessCode: code);
        if (!root.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Object) throw new HttpServiceException("group.get_essence", "Group essence response data is missing.");
        var messages = new List<BotGroupEssenceMessage>();
        if (data.TryGetProperty("msg_list", out var list) && list.ValueKind == JsonValueKind.Array)
            foreach (var item in list.EnumerateArray()) { var text = item.TryGetProperty("msg_content", out var content) && content.ValueKind == JsonValueKind.Array ? string.Join("", content.EnumerateArray().Where(x => x.TryGetProperty("text", out var t) && t.ValueKind == JsonValueKind.String).Select(x => x.GetProperty("text").GetString())) : string.Empty; messages.Add(new BotGroupEssenceMessage { GroupCode = Text(item, "group_code"), MessageSequence = Number(item, "msg_seq"), MessageRandom = (uint)Number(item, "msg_random"), SenderUin = Text(item, "sender_uin"), SenderNick = Text(item, "sender_nick"), SenderTime = (long)Number(item, "sender_time"), AddDigestUin = Text(item, "add_digest_uin"), AddDigestNick = Text(item, "add_digest_nick"), AddDigestTime = (long)Number(item, "add_digest_time"), CanBeRemoved = item.TryGetProperty("can_be_removed", out var removable) && removable.ValueKind == JsonValueKind.True, Text = text }); }
        return Task.FromResult(new GetGroupEssenceEventResp(new BotGroupEssenceResult { Messages = messages, IsEnd = data.TryGetProperty("is_end", out var end) && end.ValueKind == JsonValueKind.True, GroupRole = data.TryGetProperty("group_role", out var role) && role.TryGetInt32(out var roleValue) ? roleValue : 0 }));
    }
    private static string Text(JsonElement value, string name) => value.TryGetProperty(name, out var item) ? item.ToString() : string.Empty;
    private static ulong Number(JsonElement value, string name) => value.TryGetProperty(name, out var item) && item.TryGetUInt64(out var number) ? number : 0;
}
