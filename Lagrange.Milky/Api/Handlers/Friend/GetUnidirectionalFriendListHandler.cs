using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.Friend;

[ApiHandler("get_unidirectional_friend_list")]
public sealed class GetUnidirectionalFriendListHandler(BotContext lagrange) : INoRequestApiHandler<GetUnidirectionalFriendListHandler.Result>
{
    private readonly BotContext _lagrange = lagrange;

    public async ValueTask<MilkyApiResponse<Result>> HandleAsync(CancellationToken ct)
    {
        var result = await _lagrange.GetUnidirectionalFriendList(ct).WaitAsync(ct);
        return new MilkyApiResponse<Result>(new Result { Entries = result.Entries });
    }

    public sealed class Result
    {
        [JsonPropertyName("entries")] public required IReadOnlyList<IReadOnlyDictionary<string, string>> Entries { get; init; }
    }
}
