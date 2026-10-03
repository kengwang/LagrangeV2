using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.Group;

[ApiHandler("get_group_honor")]
public sealed class GetGroupHonorHandler(BotContext lagrange) : IApiHandler<GetGroupHonorHandler.Request, GetGroupHonorHandler.Result>
{
    public async ValueTask<MilkyApiResponse<Result>> HandleAsync(Request request, CancellationToken ct)
    {
        var result = await lagrange.GetGroupHonor(request.GroupId, request.HonorType, ct).WaitAsync(ct);
        return new(new Result(result));
    }

    public sealed class Request(long groupId, string honorType = "all")
    {
        [JsonPropertyName("group_id")] public long GroupId { get; init; } = groupId;
        [JsonPropertyName("honor_type")] public string HonorType { get; init; } = honorType;
    }

    public sealed class Result(Lagrange.Core.Common.Response.BotGroupHonorResult result)
    {
        [JsonPropertyName("honor_type")] public string HonorType { get; } = result.HonorType;
        [JsonPropertyName("items")] public Item[] Items { get; } = result.Items.Select(x => new Item(x)).ToArray();
    }

    public sealed class Item(Lagrange.Core.Common.Response.BotGroupHonorItem item)
    {
        [JsonPropertyName("user_id")] public long? UserId { get; } = item.UserId;
        [JsonPropertyName("nickname")] public string Nickname { get; } = item.Nickname;
        [JsonPropertyName("avatar")] public string Avatar { get; } = item.Avatar;
        [JsonPropertyName("description")] public string Description { get; } = item.Description;
    }
}
