using FastEndpoints;
using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Common.Response;
namespace Lagrange.Milky.Api.Handlers.Message;
public sealed class SynthesizeAiVoiceHandler(BotContext lagrange) : Endpoint<SynthesizeAiVoiceHandler.Request, MilkyApiResponse<SynthesizeAiVoiceHandler.Result>>
{
    public override void Configure() { AuthSchemes("Milky"); Post("/api/synthesize_ai_voice"); }
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        var value = await lagrange.SynthesizeAiVoice(request.GroupId, request.VoiceId ?? string.Empty, request.Text ?? string.Empty, request.ChatType ?? 1, ct);
        return new(new Result { ResourceId = value.FileUuid, FileName = value.FileName, Size = value.Size, Duration = value.Duration, Format = value.Format, Url = value.Url });
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
        [JsonPropertyName("url")] public string Url { get; init; } = string.Empty;
        [JsonPropertyName("resource_id")] public string ResourceId { get; init; } = string.Empty;
        [JsonPropertyName("file_name")] public string FileName { get; init; } = string.Empty;
        [JsonPropertyName("file_size")] public uint Size { get; init; }
        [JsonPropertyName("duration")] public uint Duration { get; init; }
        [JsonPropertyName("format")] public uint Format { get; init; }
    }
}
