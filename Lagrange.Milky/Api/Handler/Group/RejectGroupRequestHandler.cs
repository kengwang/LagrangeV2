using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Entity;

namespace Lagrange.Milky.Api.Handler.Group;

[Api("reject_group_request")]
public class RejectGroupRequestHandler(BotContext bot) : IEmptyResultApiHandler<RejectGroupRequestParameter>
{
    private readonly BotContext _bot = bot;

    public Task HandleAsync(RejectGroupRequestParameter parameter, CancellationToken token) => GroupRequestHelper.Handle(_bot, parameter, GroupNotificationOperate.Deny, parameter.Reason ?? string.Empty);
}

public class RejectGroupRequestParameter(long notificationSeq, string notificationType, long groupId, bool isFiltered = false, string? reason = null) : GroupRequestParameter(notificationSeq, notificationType, groupId, isFiltered)
{
    [JsonPropertyName("reason")]
    public string? Reason { get; init; } = reason;
}
