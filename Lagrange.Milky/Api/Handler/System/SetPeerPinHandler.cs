using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Exception;

namespace Lagrange.Milky.Api.Handler.System;

[Api("set_peer_pin")]
public class SetPeerPinHandler(BotContext bot) : IEmptyResultApiHandler<SetPeerPinParameter>
{
    private readonly BotContext _bot = bot;

    public Task HandleAsync(SetPeerPinParameter parameter, CancellationToken token)
    {
        return parameter.MessageScene switch
        {
            "friend" => _bot.SetPinFriend(parameter.PeerId, parameter.IsPinned),
            "group" => _bot.SetPinGroup(parameter.PeerId, parameter.IsPinned),
            "temp" => throw new ApiException(-1, "temp not supported"),
            _ => throw new ApiException(-400, $"unsupported message_scene: {parameter.MessageScene}"),
        };
    }
}

public class SetPeerPinParameter(string messageScene, long peerId, bool isPinned = true)
{
    [JsonRequired]
    [JsonPropertyName("message_scene")]
    public string MessageScene { get; init; } = messageScene;

    [JsonRequired]
    [JsonPropertyName("peer_id")]
    public long PeerId { get; init; } = peerId;

    [JsonPropertyName("is_pinned")]
    public bool IsPinned { get; init; } = isPinned;
}
