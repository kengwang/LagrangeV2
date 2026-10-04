using FastEndpoints;
using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Message;
using Lagrange.Milky.Caching;
namespace Lagrange.Milky.Api.Handlers.Message;
/// <summary>QQ extension: transcribes an existing voice message identified by scene, peer and sequence.</summary>
public sealed class TranscribeVoiceHandler(BotContext lagrange, MessageCache cache) : Endpoint<TranscribeVoiceHandler.Request, MilkyApiResponse<TranscribeVoiceHandler.Result>>
{
    public override void Configure() { AuthSchemes("Milky"); Post("/api/transcribe_voice"); }
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(request.PeerId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(request.MessageSeq);
        var type = request.MessageScene switch { "friend" => MessageType.Private, "group" => MessageType.Group, _ => throw new ArgumentException("Unsupported message scene.") };
        var sequence = checked((ulong)request.MessageSeq);
        var message = cache.Get(type, request.PeerId, sequence)
            ?? (type == MessageType.Group
                ? await lagrange.GetGroupMessage(request.PeerId, sequence, sequence).WaitAsync(ct)
                : await lagrange.GetC2CMessage(request.PeerId, sequence, sequence).WaitAsync(ct)).FirstOrDefault();
        if (message is null) return new(-404, "Message not found");
        return new(new Result { Text = await lagrange.TranscribeVoice(message, ct) });
    }
    public sealed class Request
    {
        [JsonPropertyName("message_scene")] public required string MessageScene { get; init; }
        [JsonPropertyName("peer_id")] public long PeerId { get; init; }
        [JsonPropertyName("message_seq")] public long MessageSeq { get; init; }
    }
    public sealed class Result { [JsonPropertyName("text")] public string Text { get; init; } = string.Empty; }
}
