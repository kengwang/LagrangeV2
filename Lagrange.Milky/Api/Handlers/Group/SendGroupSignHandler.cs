using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.Group;

public sealed class SendGroupSignHandler(BotContext lagrange) : Endpoint<SendGroupSignHandler.Request, MilkyApiResponse<SendGroupSignHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/send_group_sign");
    }
    private readonly BotContext _lagrange = lagrange;

    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        var result = await _lagrange.GroupClockIn(request.GroupId).WaitAsync(ct);
        return new MilkyApiResponse<Result>(new Result
        {
            IsSuccess = result.IsSuccess,
            Title = result.Title,
            KeepDayText = result.KeepDayText,
            GroupRankText = result.GroupRankText,
            ClockInTime = result.ClockInTime,
            DetailUrl = result.DetailUrl,
        });
    }

    public sealed class Request(long groupId)
    {
        [JsonPropertyName("group_id")] public required long GroupId { get; init; } = groupId;
    }

    public sealed class Result
    {
        [JsonPropertyName("is_success")] public required bool IsSuccess { get; init; }
        [JsonPropertyName("title")] public required string Title { get; init; }
        [JsonPropertyName("keep_day_text")] public required string KeepDayText { get; init; }
        [JsonPropertyName("group_rank_text")] public required string GroupRankText { get; init; }
        [JsonPropertyName("clock_in_time")] public required long ClockInTime { get; init; }
        [JsonPropertyName("detail_url")] public required string DetailUrl { get; init; }
    }
}
