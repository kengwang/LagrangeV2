using FastEndpoints;
using System.Text.Json;
using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Common.Response;

namespace Lagrange.Milky.Api.Handlers.System;

public class QzoneFeedRequest { [JsonPropertyName("feed")] public required JsonElement Feed { get; init; } }
public class QzoneToggleRequest : QzoneFeedRequest { [JsonPropertyName("enable")] public required bool Enable { get; init; } }
public class QzoneCommentRequest : QzoneFeedRequest { [JsonPropertyName("comment")] public required JsonElement Comment { get; init; } }
public sealed class QzoneFeedsPageData(BotQzonePublicFeedsPage page)
{
    [JsonPropertyName("feeds")] public IReadOnlyList<JsonElement> Feeds { get; } = page.Feeds;
    [JsonPropertyName("has_more")] public bool HasMore { get; } = page.HasMore;
    [JsonPropertyName("next_cursor")] public string? NextCursor { get; } = page.NextCursor;
}
public sealed class GetQzoneHomeFeedsHandler(BotContext bot) : Endpoint<GetQzoneHomeFeedsHandler.Request, MilkyApiResponse<QzoneFeedsPageData>>
{
    public override void Configure() { AuthSchemes("Milky"); Post("/api/get_qzone_home_feeds"); }
    public override async Task<MilkyApiResponse<QzoneFeedsPageData>> ExecuteAsync(Request req, CancellationToken ct) => new(new QzoneFeedsPageData(await bot.GetQzoneHomeFeeds(req.TargetUin, req.Cursor, ct)));
    public sealed class Request { [JsonPropertyName("target_uin")] public long? TargetUin { get; init; } [JsonPropertyName("cursor")] public string? Cursor { get; init; } }
}
public sealed class GetQzonePublicFeedsHandler(BotContext bot) : Endpoint<GetQzonePublicFeedsHandler.Request, MilkyApiResponse<QzoneFeedsPageData>>
{
    public override void Configure() { AuthSchemes("Milky"); Post("/api/get_qzone_public_feeds"); }
    public override async Task<MilkyApiResponse<QzoneFeedsPageData>> ExecuteAsync(Request req, CancellationToken ct) => new(new QzoneFeedsPageData(await bot.GetQzonePublicFeeds(req.Cursor, ct)));
    public sealed class Request { [JsonPropertyName("cursor")] public string? Cursor { get; init; } }
}
public sealed class GetQzonePublicCapabilitiesHandler(BotContext bot) : EndpointWithoutRequest<MilkyApiResponse<JsonElement>>
{
    public override void Configure() { AuthSchemes("Milky"); Post("/api/get_qzone_public_capabilities"); }
    public override async Task<MilkyApiResponse<JsonElement>> ExecuteAsync(CancellationToken ct) => new(await bot.GetQzonePublicCapabilities(ct));
}
public sealed class GetQzonePublicFeedDetailHandler(BotContext bot) : Endpoint<QzoneFeedRequest, MilkyApiResponse<JsonElement>>
{
    public override void Configure() { AuthSchemes("Milky"); Post("/api/get_qzone_public_feed_detail"); }
    public override async Task<MilkyApiResponse<JsonElement>> ExecuteAsync(QzoneFeedRequest req, CancellationToken ct) => new(await bot.GetQzonePublicFeedDetail(req.Feed, ct));
}
public sealed class GetQzonePublicCommentsHandler(BotContext bot) : Endpoint<GetQzonePublicCommentsHandler.Request, MilkyApiResponse<JsonElement>>
{
    public override void Configure() { AuthSchemes("Milky"); Post("/api/get_qzone_public_comments"); }
    public override async Task<MilkyApiResponse<JsonElement>> ExecuteAsync(Request req, CancellationToken ct) => new(await bot.GetQzonePublicComments(req.Feed, req.Cursor, ct));
    public sealed class Request : QzoneFeedRequest { [JsonPropertyName("cursor")] public string Cursor { get; set; } = string.Empty; }
}
public sealed class GetQzonePublicRepliesHandler(BotContext bot) : Endpoint<GetQzonePublicRepliesHandler.Request, MilkyApiResponse<JsonElement>>
{
    public override void Configure() { AuthSchemes("Milky"); Post("/api/get_qzone_public_replies"); }
    public override async Task<MilkyApiResponse<JsonElement>> ExecuteAsync(Request req, CancellationToken ct) => new(await bot.GetQzonePublicReplies(req.Feed, req.Comment, req.Cursor, req.PageSize, ct));
    public sealed class Request : QzoneCommentRequest { [JsonPropertyName("cursor")] public string Cursor { get; set; } = string.Empty; [JsonPropertyName("page_size")] public int PageSize { get; set; } = 20; }
}
public sealed class SetQzonePublicUpvoteHandler(BotContext bot) : Endpoint<QzoneToggleRequest, MilkyApiResponse<JsonElement>>
{
    public override void Configure() { AuthSchemes("Milky"); Post("/api/set_qzone_public_upvote"); }
    public override async Task<MilkyApiResponse<JsonElement>> ExecuteAsync(QzoneToggleRequest req, CancellationToken ct) => new(await bot.SetQzonePublicUpvote(req.Feed, req.Enable, ct));
}
public sealed class SetQzonePublicLikeHandler(BotContext bot) : Endpoint<QzoneToggleRequest, MilkyApiResponse<JsonElement>>
{
    public override void Configure() { AuthSchemes("Milky"); Post("/api/set_qzone_public_like"); }
    public override async Task<MilkyApiResponse<JsonElement>> ExecuteAsync(QzoneToggleRequest req, CancellationToken ct) => new(await bot.SetQzonePublicLike(req.Feed, req.Enable, ct));
}
public sealed class SetQzonePublicCommentLikeHandler(BotContext bot) : Endpoint<SetQzonePublicCommentLikeHandler.Request, MilkyApiResponse<JsonElement>>
{
    public override void Configure() { AuthSchemes("Milky"); Post("/api/set_qzone_public_comment_like"); }
    public override async Task<MilkyApiResponse<JsonElement>> ExecuteAsync(Request req, CancellationToken ct) => new(await bot.SetQzonePublicCommentLike(req.Feed, req.Comment, req.Enable, req.Reply, ct));
    public sealed class Request : QzoneCommentRequest { [JsonPropertyName("enable")] public required bool Enable { get; init; } [JsonPropertyName("reply")] public JsonElement? Reply { get; init; } }
}
public sealed class CommentQzonePublicFeedHandler(BotContext bot) : Endpoint<CommentQzonePublicFeedHandler.Request, MilkyApiResponse<JsonElement>>
{
    public override void Configure() { AuthSchemes("Milky"); Post("/api/comment_qzone_public_feed"); }
    public override async Task<MilkyApiResponse<JsonElement>> ExecuteAsync(Request req, CancellationToken ct) => new(await bot.CommentQzonePublicFeed(req.Feed, req.Content, ct));
    public sealed class Request : QzoneFeedRequest { [JsonPropertyName("content")] public required JsonElement Content { get; init; } }
}
public sealed class ReplyQzonePublicFeedHandler(BotContext bot) : Endpoint<ReplyQzonePublicFeedHandler.Request, MilkyApiResponse<JsonElement>>
{
    public override void Configure() { AuthSchemes("Milky"); Post("/api/reply_qzone_public_feed"); }
    public override async Task<MilkyApiResponse<JsonElement>> ExecuteAsync(Request req, CancellationToken ct) => new(await bot.ReplyQzonePublicFeed(req.Feed, req.Comment, req.Content, req.Reply, ct));
    public sealed class Request : QzoneCommentRequest { [JsonPropertyName("content")] public required JsonElement Content { get; init; } [JsonPropertyName("reply")] public JsonElement? Reply { get; init; } }
}
public sealed class DeleteQzonePublicCommentHandler(BotContext bot) : Endpoint<QzoneCommentRequest, MilkyApiResponse<JsonElement>>
{
    public override void Configure() { AuthSchemes("Milky"); Post("/api/delete_qzone_public_comment"); }
    public override async Task<MilkyApiResponse<JsonElement>> ExecuteAsync(QzoneCommentRequest req, CancellationToken ct) => new(await bot.DeleteQzonePublicComment(req.Feed, req.Comment, ct));
}
public sealed class DeleteQzonePublicReplyHandler(BotContext bot) : Endpoint<DeleteQzonePublicReplyHandler.Request, MilkyApiResponse<JsonElement>>
{
    public override void Configure() { AuthSchemes("Milky"); Post("/api/delete_qzone_public_reply"); }
    public override async Task<MilkyApiResponse<JsonElement>> ExecuteAsync(Request req, CancellationToken ct) => new(await bot.DeleteQzonePublicReply(req.Feed, req.Comment, req.Reply, ct));
    public sealed class Request : QzoneCommentRequest { [JsonPropertyName("reply")] public required JsonElement Reply { get; init; } }
}
public sealed class SetQzonePublicFollowHandler(BotContext bot) : Endpoint<QzoneToggleRequest, MilkyApiResponse<JsonElement>>
{
    public override void Configure() { AuthSchemes("Milky"); Post("/api/set_qzone_public_follow"); }
    public override async Task<MilkyApiResponse<JsonElement>> ExecuteAsync(QzoneToggleRequest req, CancellationToken ct) => new(await bot.SetQzonePublicFollow(req.Feed, req.Enable, ct));
}
public sealed class ForwardQzonePublicFeedHandler(BotContext bot) : Endpoint<ForwardQzonePublicFeedHandler.Request, MilkyApiResponse<JsonElement>>
{
    public override void Configure() { AuthSchemes("Milky"); Post("/api/forward_qzone_public_feed"); }
    public override async Task<MilkyApiResponse<JsonElement>> ExecuteAsync(Request req, CancellationToken ct) => new(await bot.ForwardQzonePublicFeed(req.Feed, req.Reason, ct));
    public sealed class Request : QzoneFeedRequest { [JsonPropertyName("reason")] public string Reason { get; set; } = string.Empty; }
}
public sealed class GetQzoneShareArkHandler(BotContext bot) : Endpoint<QzoneFeedRequest, MilkyApiResponse<GetQzoneShareArkHandler.Result>>
{
    public override void Configure() { AuthSchemes("Milky"); Post("/api/get_qzone_share_ark"); }
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(QzoneFeedRequest req, CancellationToken ct)
    { var value = await bot.GetQzoneShareArk(req.Feed, ct); return new(new Result { Ark = value.Ark, RequiresConfirmation = value.RequiresConfirmation, Notice = value.Notice }); }
    public sealed class Result { [JsonPropertyName("ark")] public required string Ark { get; init; } [JsonPropertyName("requires_confirmation")] public bool RequiresConfirmation { get; init; } [JsonPropertyName("notice")] public string? Notice { get; init; } }
}
