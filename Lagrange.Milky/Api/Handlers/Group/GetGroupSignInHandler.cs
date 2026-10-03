using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.Group;

[ApiHandler("get_group_sign_in")]
public sealed class GetGroupSignInHandler(BotContext lagrange) : IApiHandler<GetGroupSignInHandler.Request, GetGroupSignInHandler.Result>
{
    public async ValueTask<MilkyApiResponse<Result>> HandleAsync(Request request, CancellationToken ct)
    {
        var result = await lagrange.GetGroupSignIn(request.GroupId, request.Day, ct).WaitAsync(ct);
        return new(new Result(result));
    }
    public sealed class Request(long groupId, DateTime? day = null)
    {
        [JsonPropertyName("group_id")] public long GroupId { get; init; } = groupId;
        [JsonPropertyName("day")] public DateTime? Day { get; init; } = day;
    }
    public sealed class Result(Lagrange.Core.Common.Response.BotGroupSignInResult result)
    {
        [JsonPropertyName("members")] public Item[] Members { get; } = result.Members.Select(x => new Item(x)).ToArray();
    }
    public sealed class Item(Lagrange.Core.Common.Response.BotGroupSignInMember item)
    {
        [JsonPropertyName("user_id")] public long UserId { get; } = item.UserId;
        [JsonPropertyName("nickname")] public string Nickname { get; } = item.Nickname;
        [JsonPropertyName("time")] public long Time { get; } = item.Time;
        [JsonPropertyName("rank")] public int Rank { get; } = item.Rank;
    }
}
