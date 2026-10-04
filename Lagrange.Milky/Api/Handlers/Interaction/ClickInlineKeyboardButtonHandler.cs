using FastEndpoints;
using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
namespace Lagrange.Milky.Api.Handlers.Interaction;
/// <summary>QQ extension endpoint: click_inline_keyboard_button.</summary>
public sealed class ClickInlineKeyboardButtonHandler(BotContext lagrange) : Endpoint<ClickInlineKeyboardButtonHandler.Request, MilkyApiResponse<ClickInlineKeyboardButtonHandler.Result>>
{
    public override void Configure() { AuthSchemes("Milky"); Post("/api/click_inline_keyboard_button"); }
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        var result = await lagrange.ClickInlineKeyboardButton(request.GroupId, request.AppId, request.MessageSeq, request.ButtonId ?? string.Empty, request.CallbackData ?? string.Empty, ct);
        return new(new Result { PromptText = result.PromptText });
    }
    public sealed class Request
    {
        [JsonPropertyName("group_id")] public long GroupId { get; init; }
        [JsonPropertyName("app_id")] public ulong AppId { get; init; }
        [JsonPropertyName("message_seq")] public ulong MessageSeq { get; init; }
        [JsonPropertyName("button_id")] public string? ButtonId { get; init; }
        [JsonPropertyName("callback_data")] public string? CallbackData { get; init; }
    }
    public sealed class Result
    {
        [JsonPropertyName("prompt_text")] public string PromptText { get; init; } = string.Empty;
    }
}
