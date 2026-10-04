using FastEndpoints;
using System;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.System;

public sealed class SetPeerPinHandler(BotContext lagrange) : Endpoint<SetPeerPinHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/set_peer_pin");
    }
    private readonly BotContext _lagrange = lagrange;

    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
    {
        await (request.MessageScene switch
        {
            "friend" => _lagrange.SetPinFriend(request.PeerId, request.IsPinned, ct),
            "group" => _lagrange.SetPinGroup(request.PeerId, request.IsPinned, ct),
            _ => throw new NotSupportedException(),
        });
        return new MilkyApiResponse();
    }

    public sealed class Request(string messageScene, long peerId, bool isPinned = true)
    {
        [JsonPropertyName("message_scene")] public required string MessageScene { get; init; } = messageScene;
        [JsonPropertyName("peer_id")] public required long PeerId { get; init; } = peerId;
        [JsonPropertyName("is_pinned")] public bool IsPinned { get; init; } = isPinned;
    }
}
