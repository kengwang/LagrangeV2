using System.Text.Json.Serialization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Common.Response;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.Message;

[ApiHandler("get_collection")]
public sealed class GetCollectionHandler(BotContext lagrange) : IApiHandler<GetCollectionHandler.Request, GetCollectionHandler.Result>
{
    public async ValueTask<MilkyApiResponse<Result>> HandleAsync(Request request, CancellationToken ct)
    {
        var result = await lagrange.GetCollection(request.Count, ct).WaitAsync(ct);
        return new(new Result(result));
    }
    public sealed class Request(uint count = 50) { [JsonPropertyName("count")] public uint Count { get; init; } = count; }
    public sealed class Result(BotCollectionResult result)
    {
        [JsonPropertyName("items")] public Item[] Items { get; } = result.Items.Select(x => new Item(x)).ToArray();
        [JsonPropertyName("total_count")] public uint TotalCount { get; } = result.TotalCount;
        [JsonPropertyName("reached_bottom")] public bool ReachedBottom { get; } = result.ReachedBottom;
    }
    public sealed class Item(BotCollectionItem item)
    {
        [JsonPropertyName("id")] public string Id { get; } = item.Id;
        [JsonPropertyName("type")] public uint Type { get; } = item.Type;
        [JsonPropertyName("text")] public string? Text { get; } = item.Text;
        [JsonPropertyName("share_url")] public string? ShareUrl { get; } = item.ShareUrl;
        [JsonPropertyName("create_time")] public ulong CreateTime { get; } = item.CreateTime;
        [JsonPropertyName("collect_time")] public ulong CollectTime { get; } = item.CollectTime;
        [JsonPropertyName("modify_time")] public ulong ModifyTime { get; } = item.ModifyTime;
        [JsonPropertyName("author_uid")] public string? AuthorUid { get; } = item.AuthorUid;
        [JsonPropertyName("author_uin")] public long AuthorUin { get; } = item.AuthorUin;
    }
}
