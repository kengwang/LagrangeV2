using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Message;
using Lagrange.Milky.Api.Exception;

namespace Lagrange.Milky.Api.Handler.Message;

[Api("mark_message_as_read")]
public class MarkMessageAsReadHandler(BotContext bot) : IEmptyResultApiHandler<MarkMessageAsReadParameter>
{
    private readonly BotContext _bot = bot;

    public Task HandleAsync(MarkMessageAsReadParameter parameter, CancellationToken token)
    {
        MessageType type = parameter.MessageScene switch
        {
            "friend" => MessageType.Private,
            "group" => MessageType.Group,
            "temp" => throw new ApiException(-1, "temp not supported"),
            _ => throw new ApiException(-400, $"unsupported message_scene: {parameter.MessageScene}"),
        };

        return _bot.MarkAsRead(type, parameter.PeerId, (ulong)parameter.MessageSeq);
    }
}

public class MarkMessageAsReadParameter(string messageScene, long peerId, long messageSeq)
{
    [JsonRequired]
    [JsonPropertyName("message_scene")]
    public string MessageScene { get; init; } = messageScene;

    [JsonRequired]
    [JsonPropertyName("peer_id")]
    public long PeerId { get; init; } = peerId;

    [JsonRequired]
    [JsonPropertyName("message_seq")]
    public long MessageSeq { get; init; } = messageSeq;
}
