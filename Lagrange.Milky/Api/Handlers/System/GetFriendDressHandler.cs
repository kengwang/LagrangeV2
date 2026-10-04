using FastEndpoints;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.System;

public sealed class GetFriendDressHandler(BotContext lagrange) : Endpoint<GetFriendDressHandler.Request, MilkyApiResponse<GetFriendDressHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/get_friend_dress");
    }
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        var result = await lagrange.GetFriendDress(request.TargetUin, ct).WaitAsync(ct);
        return new(new Result(result));
    }
    public sealed class Request(long targetUin) { [JsonPropertyName("target_uin")] public long TargetUin { get; init; } = targetUin; }
    public sealed class Result(Lagrange.Core.Common.Response.BotFriendDressResult result)
    {
        [JsonPropertyName("target_uin")] public string TargetUin { get; } = result.TargetUin;
        [JsonPropertyName("is_svip")] public bool IsSvip { get; } = result.IsSvip;
        [JsonPropertyName("avatar_url")] public string AvatarUrl { get; } = result.AvatarUrl;
        [JsonPropertyName("items")] public Item[] Items { get; } = result.Items.Select(x => new Item(x)).ToArray();
    }
    public sealed class Item(Lagrange.Core.Common.Response.BotFriendDressItem item)
    {
        [JsonPropertyName("app_id")] public int AppId { get; } = item.AppId;
        [JsonPropertyName("kind")] public string Kind { get; } = item.Kind;
        [JsonPropertyName("item_id")] public int ItemId { get; } = item.ItemId;
        [JsonPropertyName("name")] public string Name { get; } = item.Name;
        [JsonPropertyName("preview_url")] public string PreviewUrl { get; } = item.PreviewUrl;
        [JsonPropertyName("video_url")] public string VideoUrl { get; } = item.VideoUrl;
        [JsonPropertyName("price")] public int Price { get; } = item.Price;
    }
}
