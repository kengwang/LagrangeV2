using Lagrange.Core.Common;
using System.Text;
using System.Text.Json;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Http;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.Http;

[HttpServiceAttribute("group.get_sign_in", "POST", "/v2/signin/trpc/GetDaySignedList", "qun.qq.com")]
[EventSubscribe<GetGroupSignInEventReq>(Protocols.All)]
internal sealed class GroupSignHttpService : HttpService<GetGroupSignInEventReq, GetGroupSignInEventResp>
{
    public GroupSignHttpService() : base("qun.qq.com") { }
    protected override async Task<HttpRequestMessage> BuildRequestAsync(BotContext context, GetGroupSignInEventReq request, CancellationToken cancellationToken)
    {
        if (request.GroupUin <= 0) throw new ArgumentOutOfRangeException(nameof(request.GroupUin));
        var pskey = await context.HttpSessionContext.GetPSkeyAsync("qun.qq.com", cancellationToken);
        var date = (request.Day ?? DateTime.Now).ToString("yyyyMMdd");
        var payload = $"{{\"dayYmd\":\"{date}\",\"offset\":0,\"limit\":100,\"uid\":\"{context.BotUin}\",\"groupId\":\"{request.GroupUin}\"}}";
        return new HttpRequestMessage(HttpMethod.Post, $"https://qun.qq.com/v2/signin/trpc/GetDaySignedList?g_tk={ComputeBkn(pskey)}") { Content = new StringContent(payload, Encoding.UTF8, "application/json") };
    }
    protected override Task<GetGroupSignInEventResp> ParseResponseAsync(BotContext context, GetGroupSignInEventReq request, HttpResponseMessage response, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken)
    {
        using var document = HttpResponseParser.ParseJson(payload, "group.get_sign_in"); var root = document.RootElement;
        if (!root.TryGetProperty("response", out var responseRoot) || !responseRoot.TryGetProperty("page", out var pages) || pages.ValueKind != JsonValueKind.Array) throw new HttpServiceException("group.get_sign_in", "Group sign-in response page is missing.");
        var members = new List<BotGroupSignInMember>(); var first = pages.EnumerateArray().FirstOrDefault();
        if (first.ValueKind == JsonValueKind.Object && first.TryGetProperty("infos", out var infos) && infos.ValueKind == JsonValueKind.Array)
            foreach (var item in infos.EnumerateArray()) { var raw = (int)Number(item, "signInRank"); members.Add(new BotGroupSignInMember { UserId = (long)Number(item, "uid"), Nickname = Text(item, "uidGroupNick"), Time = (long)Number(item, "signedTimeStamp"), Rank = raw > 0 ? (raw - 1) / 2 + 1 : 0 }); }
        return Task.FromResult(new GetGroupSignInEventResp(new BotGroupSignInResult { Members = members }));
    }
    private static string Text(JsonElement value, string name) => value.TryGetProperty(name, out var item) ? item.ToString() : string.Empty;
    private static ulong Number(JsonElement value, string name) => value.TryGetProperty(name, out var item) && item.TryGetUInt64(out var number) ? number : 0;
    private static string ComputeBkn(string skey) { uint hash = 5381; foreach (var c in skey) hash += (hash << 5) + c; return (hash & 0x7FFFFFFF).ToString(); }
}
