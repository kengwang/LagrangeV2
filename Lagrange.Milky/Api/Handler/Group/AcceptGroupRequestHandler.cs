using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Exception;

namespace Lagrange.Milky.Api.Handler.Group;

[Api("accept_group_request")]
public class AcceptGroupRequestHandler(BotContext bot) : IEmptyResultApiHandler<GroupRequestParameter>
{
    private readonly BotContext _bot = bot;

    public Task HandleAsync(GroupRequestParameter parameter, CancellationToken token) => GroupRequestHelper.Handle(_bot, parameter, GroupNotificationOperate.Allow);
}

public class GroupRequestParameter(long notificationSeq, string notificationType, long groupId, bool isFiltered = false)
{
    [JsonRequired]
    [JsonPropertyName("notification_seq")]
    public long NotificationSeq { get; init; } = notificationSeq;

    [JsonRequired]
    [JsonPropertyName("notification_type")]
    public string NotificationType { get; init; } = notificationType;

    [JsonRequired]
    [JsonPropertyName("group_id")]
    public long GroupId { get; init; } = groupId;

    [JsonPropertyName("is_filtered")]
    public bool IsFiltered { get; init; } = isFiltered;
}

internal static class GroupRequestHelper
{
    public static Task Handle(BotContext bot, GroupRequestParameter parameter, GroupNotificationOperate operate, string message = "")
    {
        BotGroupNotificationType type = parameter.NotificationType switch
        {
            "join_request" => BotGroupNotificationType.Join,
            "invited_join_request" => BotGroupNotificationType.Invite,
            _ => throw new ApiException(-400, $"unsupported notification_type: {parameter.NotificationType}"),
        };

        return bot.SetGroupNotification(parameter.GroupId, (ulong)parameter.NotificationSeq, type, parameter.IsFiltered, operate, message);
    }
}
