using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Exception;
using Lagrange.Milky.Utility;

namespace Lagrange.Milky.Api.Handler.System;

[Api("set_avatar")]
public class SetAvatarHandler(BotContext bot, ResourceResolver resolver) : IEmptyResultApiHandler<SetAvatarParameter>
{
    private readonly BotContext _bot = bot;
    private readonly ResourceResolver _resolver = resolver;

    public async Task HandleAsync(SetAvatarParameter parameter, CancellationToken token)
    {
        await using var stream = await _resolver.ToMemoryStreamAsync(parameter.Uri, token);
        if (!await _bot.SetBotAvatar(stream)) throw new ApiException(-1, "set avatar failed");
    }
}

public class SetAvatarParameter(string uri)
{
    [JsonRequired]
    [JsonPropertyName("uri")]
    public string Uri { get; init; } = uri;
}
