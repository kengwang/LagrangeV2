using FastEndpoints;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Converters;
using Lagrange.Milky.Models;

namespace Lagrange.Milky.Api.Handlers.Group;

public sealed class GetGroupNotificationsHandler(BotContext lagrange, MilkyConverter converter) : Endpoint<GetGroupNotificationsHandler.Request, MilkyApiResponse<GetGroupNotificationsHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/get_group_notifications");
    }
    private readonly BotContext _lagrange = lagrange;
    private readonly MilkyConverter _converter = converter;

    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        var notifications = await (request.IsFiltered switch
        {
            true => _lagrange.FetchFilteredGroupNotifications(
                (ulong)request.Limit,
                (ulong?)request.StartNotificationSeq ?? 0,
                ct
            ).WaitAsync(ct),
            false => _lagrange.FetchGroupNotifications(
                (ulong)request.Limit,
                (ulong?)request.StartNotificationSeq ?? 0,
                ct
            ).WaitAsync(ct),
        });

        return new MilkyApiResponse<Result>(new Result
        {
            Notifications = [.. notifications.Select(_converter.ToGroupNotification)],
            NextNotificationSeq = null,
        });
    }

    public sealed class Request(long? startNotificationSeq = null, bool isFiltered = false, int limit = 20)
    {
        [JsonPropertyName("start_notification_seq")] public long? StartNotificationSeq { get; init; } = startNotificationSeq;
        [JsonPropertyName("is_filtered")] public bool IsFiltered { get; init; } = isFiltered;
        [JsonPropertyName("limit")] public int Limit { get; init; } = limit;
    }

    public sealed class Result
    {
        [JsonPropertyName("notifications")] public required IReadOnlyList<GroupNotificationBase> Notifications { get; init; }
        [JsonPropertyName("next_notification_seq")] public required long? NextNotificationSeq { get; init; }
    }
}
