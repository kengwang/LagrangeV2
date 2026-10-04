using FastEndpoints;
using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Common.Response;
namespace Lagrange.Milky.Api.Handlers.Message;
public sealed class SendAiVoiceHandler(BotContext lagrange) : Endpoint<SendAiVoiceHandler.Request, MilkyApiResponse<SendAiVoiceHandler.Result>>
{
    public override void Configure() { AuthSchemes("Milky"); Post("/api/send_ai_voice"); }
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        var value = await lagrange.SendAiVoice(request.GroupId, request.VoiceId ?? string.Empty, request.Text ?? string.Empty, request.ChatType ?? 1, ct);
        return new(new Result { MessageSeq = value.Sequence, Time = value.Time });
    }
    public sealed class Request
    {
        [JsonPropertyName("group_id")] public long GroupId { get; init; }
        [JsonPropertyName("voice_id")] public string? VoiceId { get; init; }
        [JsonPropertyName("text")] public string? Text { get; init; }
        [JsonPropertyName("chat_type")] public uint? ChatType { get; init; }
    }
    public sealed class Result
    {
        [JsonPropertyName("message_seq")] public ulong MessageSeq { get; init; }
        [JsonPropertyName("time")] public long Time { get; init; }
    }
}
