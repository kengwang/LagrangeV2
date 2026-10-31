using FastEndpoints;
using System.Text.Json;
using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.System;

public sealed class GetQzoneProfileHandler(BotContext bot) : Endpoint<GetQzoneProfileHandler.Request, MilkyApiResponse<JsonElement>>
{
    public override void Configure() { AuthSchemes("Milky"); Post("/api/get_qzone_profile"); }
    public override async Task<MilkyApiResponse<JsonElement>> ExecuteAsync(Request req, CancellationToken ct) => new(await bot.GetQzoneProfile(req.TargetUin, ct));
    public sealed class Request { [JsonPropertyName("target_uin")] public long? TargetUin { get; init; } }
}
public sealed class SetQzoneProfileHandler(BotContext bot) : Endpoint<SetQzoneProfileHandler.Request, MilkyApiResponse>
{
    public override void Configure() { AuthSchemes("Milky"); Post("/api/set_qzone_profile"); }
    public override async Task<MilkyApiResponse> ExecuteAsync(Request req, CancellationToken ct) { await bot.SetQzoneProfile(req.Name, req.Description, ct); return new(); }
    public sealed class Request { [JsonPropertyName("name")] public string? Name { get; init; } [JsonPropertyName("description")] public string? Description { get; init; } }
}
public sealed class GetQzoneMessageDetailHandler(BotContext bot) : Endpoint<GetQzoneMessageDetailHandler.Request, MilkyApiResponse<JsonElement>>
{
    public override void Configure() { AuthSchemes("Milky"); Post("/api/get_qzone_message_detail"); }
    public override async Task<MilkyApiResponse<JsonElement>> ExecuteAsync(Request req, CancellationToken ct) => new(await bot.GetQzoneMessageDetail(req.TargetUin, req.MessageId, req.Position, req.Count, ct));
    public sealed class Request
    { [JsonPropertyName("target_uin")] public required long TargetUin { get; init; } [JsonPropertyName("message_id")] public required string MessageId { get; init; } [JsonPropertyName("position")] public int Position { get; init; } [JsonPropertyName("count")] public int Count { get; set; } = 20; }
}
public sealed class ReplyQzoneCommentHandler(BotContext bot) : Endpoint<ReplyQzoneCommentHandler.Request, MilkyApiResponse<JsonElement>>
{
    public override void Configure() { AuthSchemes("Milky"); Post("/api/reply_qzone_comment"); }
    public override async Task<MilkyApiResponse<JsonElement>> ExecuteAsync(Request req, CancellationToken ct) => new(await bot.ReplyQzoneComment(req.TargetUin, req.MessageId, req.Content, req.CommentId, req.CommentUin, req.ReplyUin, req.ReplyName, ct));
    public sealed class Request
    {
        [JsonPropertyName("target_uin")] public required long TargetUin { get; init; }
        [JsonPropertyName("message_id")] public required string MessageId { get; init; }
        [JsonPropertyName("content")] public required string Content { get; init; }
        [JsonPropertyName("comment_id")] public required string CommentId { get; init; }
        [JsonPropertyName("comment_uin")] public required long CommentUin { get; init; }
        [JsonPropertyName("reply_uin")] public long? ReplyUin { get; init; }
        [JsonPropertyName("reply_name")] public string? ReplyName { get; init; }
    }
}
