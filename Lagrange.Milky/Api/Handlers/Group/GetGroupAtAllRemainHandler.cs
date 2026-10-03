using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.Group;

[ApiHandler("get_group_at_all_remain")]
public sealed class GetGroupAtAllRemainHandler(BotContext lagrange) : IApiHandler<GetGroupAtAllRemainHandler.Request, GetGroupAtAllRemainHandler.Result>
{
    private readonly BotContext _lagrange = lagrange;

    public async ValueTask<MilkyApiResponse<Result>> HandleAsync(Request request, CancellationToken ct)
    {
        var remain = await _lagrange.GroupRemainAtAll(request.GroupId).WaitAsync(ct);
        return new MilkyApiResponse<Result>(new Result
        {
            CanAtAll = remain.RemainAtAllCountForUin > 0 && remain.RemainAtAllCountForGroup > 0,
            RemainForUin = remain.RemainAtAllCountForUin,
            RemainForGroup = remain.RemainAtAllCountForGroup,
        });
    }

    public sealed class Request(long groupId)
    {
        [JsonPropertyName("group_id")] public required long GroupId { get; init; } = groupId;
    }

    public sealed class Result
    {
        [JsonPropertyName("can_at_all")] public required bool CanAtAll { get; init; }
        [JsonPropertyName("remain_for_uin")] public required uint RemainForUin { get; init; }
        [JsonPropertyName("remain_for_group")] public required uint RemainForGroup { get; init; }
    }
}
