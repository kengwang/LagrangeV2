using FastEndpoints;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.Group;

public sealed class GetGroupSignInHandler(BotContext lagrange) : Endpoint<GetGroupSignInHandler.Request, MilkyApiResponse<GetGroupSignInHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/get_group_sign_in");
    }
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
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
