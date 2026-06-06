using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Exception;
using Lagrange.Milky.Utility;

namespace Lagrange.Milky.Api.Handler.Group;

[Api("set_group_avatar")]
public class SetGroupAvatarHandler(BotContext bot, ResourceResolver resolver) : IEmptyResultApiHandler<SetGroupAvatarParameter>
{
    private readonly BotContext _bot = bot;
    private readonly ResourceResolver _resolver = resolver;

    public async Task HandleAsync(SetGroupAvatarParameter parameter, CancellationToken token)
    {
        await using var stream = await _resolver.ToMemoryStreamAsync(parameter.ImageUri, token);
        if (!await _bot.SetGroupAvatar(parameter.GroupId, stream)) throw new ApiException(-1, "set group avatar failed");
    }
}

public class SetGroupAvatarParameter(long groupId, string imageUri)
{
    [JsonRequired]
    [JsonPropertyName("group_id")]
    public long GroupId { get; init; } = groupId;

    [JsonRequired]
    [JsonPropertyName("image_uri")]
    public string ImageUri { get; init; } = imageUri;
}
