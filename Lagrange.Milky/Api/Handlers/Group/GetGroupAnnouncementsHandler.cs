using FastEndpoints;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.Group;

public sealed class GetGroupAnnouncementsHandler(BotContext lagrange) : Endpoint<GetGroupAnnouncementsHandler.Request, MilkyApiResponse<GetGroupAnnouncementsHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/get_group_announcements");
    }
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        var result = await lagrange.GetGroupAnnouncements(request.GroupId, request.Start, request.Count, ct).WaitAsync(ct);
        return new(new Result(result));
    }
    public sealed class Request(long groupId, int start = -1, int count = 20)
    {
        [JsonPropertyName("group_id")] public long GroupId { get; init; } = groupId;
        [JsonPropertyName("start")] public int Start { get; init; } = start;
        [JsonPropertyName("count")] public int Count { get; init; } = count;
    }
    public sealed class Result(Lagrange.Core.Common.Response.BotGroupAnnouncementResult result)
    {
        [JsonPropertyName("announcements")] public Item[] Announcements { get; } = result.Announcements.Select(x => new Item(x)).ToArray();
    }
    public sealed class Item(Lagrange.Core.Common.Response.BotGroupAnnouncement item)
    {
        [JsonPropertyName("id")] public string Id { get; } = item.Id;
        [JsonPropertyName("publisher_id")] public long PublisherId { get; } = item.PublisherId;
        [JsonPropertyName("publish_time")] public long PublishTime { get; } = item.PublishTime;
        [JsonPropertyName("text")] public string Text { get; } = item.Text;
        [JsonPropertyName("pinned")] public bool Pinned { get; } = item.Pinned;
        [JsonPropertyName("read_count")] public int ReadCount { get; } = item.ReadCount;
    }
}
