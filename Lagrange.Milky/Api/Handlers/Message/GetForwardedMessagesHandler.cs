using System.Linq;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;
using Lagrange.Milky.Converters;
using Lagrange.Milky.Models.Messages;

namespace Lagrange.Milky.Api.Handlers.Message;

[ApiHandler("get_forwarded_messages")]
public sealed class GetForwardedMessagesHandler(BotContext lagrange, MilkyConverter converter) : IApiHandler<GetForwardedMessagesHandler.Request, GetForwardedMessagesHandler.Result>
{
    public async ValueTask<MilkyApiResponse<Result>> HandleAsync(Request request, CancellationToken ct)
    {
        var messages = await lagrange.GetForwardedMessages(request.ForwardId, request.MessageScene == "group", ct).WaitAsync(ct);
        var result = new List<IncomingForwardedMessage>(messages.Count);
        foreach (var message in messages)
        {
            var type = message.Type;
            var peer = message.Contact switch { BotGroupMember member => member.Group.GroupUin, BotFriend friend => friend.Uin, _ => message.Receiver.Uin };
            result.Add(new IncomingForwardedMessage
            {
                MessageSeq = checked((long)message.Sequence),
                SenderName = message.Contact.Nickname,
                AvatarUrl = string.Empty,
                Time = message.Time,
                Segments = await converter.ToIncomingSegmentsAsync(message.Entities, type, peer, ct),
            });
        }
        return new(new Result { Messages = result.ToArray() });
    }

    public sealed class Request(string forwardId, string messageScene = "friend")
    {
        [JsonPropertyName("forward_id")] public required string ForwardId { get; init; } = forwardId;
        [JsonPropertyName("message_scene")] public string MessageScene { get; init; } = messageScene;
    }
    public sealed class Result { [JsonPropertyName("messages")] public required IncomingForwardedMessage[] Messages { get; init; } }
}
