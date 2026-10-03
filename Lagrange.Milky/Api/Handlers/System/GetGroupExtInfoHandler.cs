using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.System;

[ApiHandler("get_group_ext_info")]
public sealed class GetGroupExtInfoHandler(BotContext lagrange) : IApiHandler<GetGroupExtInfoHandler.Request, GetGroupExtInfoHandler.Result>
{
    private readonly BotContext _lagrange = lagrange;

    public async ValueTask<MilkyApiResponse<Result>> HandleAsync(Request request, CancellationToken ct)
    {
        var extra = await _lagrange.FetchGroupExtra(request.GroupId, ct).WaitAsync(ct);
        return new MilkyApiResponse<Result>(new Result { LatestMessageSequence = extra.LatestMessageSequence });
    }

    public sealed class Request(long groupId)
    {
        [JsonPropertyName("group_id")] public required long GroupId { get; init; } = groupId;
    }

    public sealed class Result
    {
        [JsonPropertyName("latest_message_sequence")] public required long LatestMessageSequence { get; init; }
    }
}
