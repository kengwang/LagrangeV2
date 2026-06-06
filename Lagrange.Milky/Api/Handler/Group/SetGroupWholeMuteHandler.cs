using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handler.Group;

[Api("set_group_whole_mute")]
public class SetGroupWholeMuteHandler(BotContext bot) : IEmptyResultApiHandler<SetGroupWholeMuteParameter>
{
    private readonly BotContext _bot = bot;

    public Task HandleAsync(SetGroupWholeMuteParameter parameter, CancellationToken token)
    {
        return _bot.GroupMuteGlobal(parameter.GroupId, parameter.IsMute);
    }
}

public class SetGroupWholeMuteParameter(long groupId, bool isMute = true)
{
    [JsonRequired]
    [JsonPropertyName("group_id")]
    public long GroupId { get; init; } = groupId;

    [JsonPropertyName("is_mute")]
    public bool IsMute { get; init; } = isMute;
}
