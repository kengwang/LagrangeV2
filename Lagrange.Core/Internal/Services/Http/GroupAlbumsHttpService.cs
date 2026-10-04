using Lagrange.Core.Common;
using System.Text.Json;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Http;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.Http;

[HttpServiceAttribute("group.get_albums", "GET", "/proxy/domain/u.photo.qzone.qq.com/cgi-bin/upp/qun_list_album_v2", "qzone.qq.com", AuthInjection = HttpAuthInjection.UrlBknFromPSkey)]
[EventSubscribe<GetGroupAlbumsEventReq>(Protocols.All)]
internal sealed class GroupAlbumsHttpService : HttpService<GetGroupAlbumsEventReq, GetGroupAlbumsEventResp>
{
    protected override async Task<HttpRequestMessage> BuildRequestAsync(BotContext context, GetGroupAlbumsEventReq request, CancellationToken cancellationToken)
    {
        if (request.GroupUin <= 0 || request.AttachInfo.Length > 4096) throw new ArgumentOutOfRangeException(nameof(request));
        var query = $"random=7570&format=json&inCharset=utf-8&outCharset=utf-8&qua=V1_IPH_SQ_6.2.0_0_HDBM_T&cmd=qunGetAlbumList&qunId={request.GroupUin}&qunid={request.GroupUin}&start=0&num=1000&uin={context.BotUin}&getMemberRole=0";
        return await CreateGetRequestAsync(context, new Uri($"https://h5.qzone.qq.com/proxy/domain/u.photo.qzone.qq.com/cgi-bin/upp/qun_list_album_v2?{query}"), cancellationToken);
    }
    protected override Task<GetGroupAlbumsEventResp> ParseResponseAsync(BotContext context, GetGroupAlbumsEventReq request, HttpResponseMessage response, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken)
    {
        using var document = HttpResponseParser.ParseJson(payload, "group.get_albums");
        var root = document.RootElement;
        var data = root.TryGetProperty("data", out var dataElement) ? dataElement : root;
        var albums = new List<BotGroupAlbum>();
        if (data.TryGetProperty("album", out var array) && array.ValueKind == JsonValueKind.Array)
            foreach (var item in array.EnumerateArray())
            {
                var id = Text(item, "id");
                if (string.IsNullOrWhiteSpace(id)) continue;
                albums.Add(new BotGroupAlbum { AlbumId = id, Name = Text(item, "name"), Description = Text(item, "desc"), Owner = Text(item, "owner"), CreateTime = Number(item, "createTime"), UploadNumber = Number(item, "picNum") });
            }
        return Task.FromResult(new GetGroupAlbumsEventResp(new BotGroupAlbumResult { Albums = albums, HasMore = false, AttachInfo = request.AttachInfo }));
    }
    private static string Text(JsonElement value, string name) => value.TryGetProperty(name, out var item) ? item.ToString() : string.Empty;
    private static ulong Number(JsonElement value, string name) => value.TryGetProperty(name, out var item) && item.TryGetUInt64(out var number) ? number : 0;
}
