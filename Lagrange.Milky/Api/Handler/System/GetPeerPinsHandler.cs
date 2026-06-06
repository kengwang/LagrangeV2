using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Utility;

namespace Lagrange.Milky.Api.Handler.System;

[Api("get_peer_pins")]
public class GetPeerPinsHandler(BotContext bot, EntityConvert convert) : IEmptyParameterApiHandler<GetPeerPinsResult>
{
    private readonly BotContext _bot = bot;
    private readonly EntityConvert _convert = convert;

    public async Task<GetPeerPinsResult> HandleAsync(CancellationToken token)
    {
        var pins = await _bot.FetchPins();
        return new GetPeerPinsResult(
            pins.Friends.Select(_convert.Friend),
            pins.Groups.Select(_convert.Group)
        );
    }
}

public class GetPeerPinsResult(IEnumerable<Entity.Friend> friends, IEnumerable<Entity.Group> groups)
{
    [JsonPropertyName("friends")]
    public IEnumerable<Entity.Friend> Friends { get; } = friends;

    [JsonPropertyName("groups")]
    public IEnumerable<Entity.Group> Groups { get; } = groups;
}
