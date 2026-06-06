using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handler.Group;

[Api("set_group_essence_message")]
public class SetGroupEssenceMessageHandler(BotContext bot) : IEmptyResultApiHandler<SetGroupEssenceMessageParameter>
{
    private readonly BotContext _bot = bot;

    public Task HandleAsync(SetGroupEssenceMessageParameter parameter, CancellationToken token)
    {
        return _bot.SetGroupEssenceMessage(parameter.GroupId, (ulong)parameter.MessageSeq, parameter.IsSet);
    }
}

public class SetGroupEssenceMessageParameter(long groupId, long messageSeq, bool isSet = true)
{
    [JsonRequired]
    [JsonPropertyName("group_id")]
    public long GroupId { get; init; } = groupId;

    [JsonRequired]
    [JsonPropertyName("message_seq")]
    public long MessageSeq { get; init; } = messageSeq;

    [JsonPropertyName("is_set")]
    public bool IsSet { get; init; } = isSet;
}
