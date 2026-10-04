using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.System;

public sealed class SetQzoneLikeHandler(BotContext lagrange) : Endpoint<SetQzoneLikeHandler.Request, MilkyApiResponse<SetQzoneLikeHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/set_qzone_like");
    }
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        await lagrange.SetQzoneLike(request.TargetUin, request.MessageId, request.Like, request.AbsTime, ct).WaitAsync(ct);
        return new(new Result());
    }

    public sealed class Request(long targetUin, string messageId, bool like = true, long absTime = 0)
    {
        [JsonPropertyName("target_uin")] public long TargetUin { get; init; } = targetUin;
        [JsonPropertyName("message_id")] public required string MessageId { get; init; } = messageId;
        [JsonPropertyName("like")] public bool Like { get; init; } = like;
        [JsonPropertyName("abstime")] public long AbsTime { get; init; } = absTime;
    }

    public sealed class Result;
}
