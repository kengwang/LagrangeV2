using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entities;
using Lagrange.Milky.Entity.Message;
using Lagrange.Milky.Utility;

namespace Lagrange.Milky.Api.Handler.Message;

[Api("get_forwarded_messages")]
public class GetForwardedMessagesHandler(BotContext bot, EntityConvert convert) : IApiHandler<GetForwardedMessagesParameter, GetForwardedMessagesResult>
{
    private readonly BotContext _bot = bot;
    private readonly EntityConvert _convert = convert;

    public async Task<GetForwardedMessagesResult> HandleAsync(GetForwardedMessagesParameter parameter, CancellationToken token)
    {
        var entity = new MultiMsgEntity(parameter.ForwardId);
        var message = BotMessage.CreateCustomFriend(_bot.BotUin, string.Empty, _bot.BotUin, string.Empty, DateTime.Now, new MessageChain { entity });
        await entity.Postprocess(_bot, message);

        return new GetForwardedMessagesResult(entity.Messages.Select(_convert.IncomingForwardedMessage));
    }
}

public class GetForwardedMessagesParameter(string forwardId)
{
    [JsonRequired]
    [JsonPropertyName("forward_id")]
    public string ForwardId { get; init; } = forwardId;
}

public class GetForwardedMessagesResult(IEnumerable<IncomingForwardedMessage> messages)
{
    [JsonPropertyName("messages")]
    public IEnumerable<IncomingForwardedMessage> Messages { get; } = messages;
}
