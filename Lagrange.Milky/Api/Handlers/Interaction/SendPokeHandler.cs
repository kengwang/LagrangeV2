using FastEndpoints;
using System;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.Interaction;

public sealed class SendPokeHandler(BotContext lagrange) : Endpoint<SendPokeHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/send_poke");
    }
    private readonly BotContext _lagrange = lagrange;

    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
    {
        switch (request.MessageScene)
        {
            case "friend":
                await _lagrange.SendFriendNudge(request.PeerId, request.TargetUserId, ct);
                break;
            case "group":
                if (request.TargetUserId is not { } target) throw new ArgumentException("target_user_id is required for group poke");
                await _lagrange.SendGroupNudge(request.PeerId, target, ct);
                break;
            default:
                throw new ArgumentException($"Unsupported message scene: {request.MessageScene}");
        }

        return new MilkyApiResponse();
    }

    public sealed class Request(string messageScene, long peerId, long? targetUserId = null)
    {
        [JsonPropertyName("message_scene")] public required string MessageScene { get; init; } = messageScene;
        [JsonPropertyName("peer_id")] public required long PeerId { get; init; } = peerId;
        [JsonPropertyName("target_user_id")] public long? TargetUserId { get; init; } = targetUserId;
    }
}
