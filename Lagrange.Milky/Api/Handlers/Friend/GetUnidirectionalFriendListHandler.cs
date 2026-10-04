using FastEndpoints;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.Friend;

public sealed class GetUnidirectionalFriendListHandler(BotContext lagrange) : EndpointWithoutRequest<MilkyApiResponse<GetUnidirectionalFriendListHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/get_unidirectional_friend_list");
    }
    private readonly BotContext _lagrange = lagrange;

    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(CancellationToken ct)
    {
        var result = await _lagrange.GetUnidirectionalFriendList(ct).WaitAsync(ct);
        return new MilkyApiResponse<Result>(new Result { Entries = result.Entries });
    }

    public sealed class Result
    {
        [JsonPropertyName("entries")] public required IReadOnlyList<IReadOnlyDictionary<string, string>> Entries { get; init; }
    }
}
