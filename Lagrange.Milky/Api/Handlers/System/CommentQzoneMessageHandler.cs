using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.System;

public sealed class CommentQzoneMessageHandler(BotContext lagrange) : Endpoint<CommentQzoneMessageHandler.Request, MilkyApiResponse<CommentQzoneMessageHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/comment_qzone_message");
    }
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        var result = await lagrange.CommentQzoneMessage(request.TargetUin, request.MessageId, request.Content, ct).WaitAsync(ct);
        return new(new Result { CommentId = result.CommentId });
    }

    public sealed class Request(long targetUin, string messageId, string content)
    {
        [JsonPropertyName("target_uin")] public long TargetUin { get; init; } = targetUin;
        [JsonPropertyName("message_id")] public required string MessageId { get; init; } = messageId;
        [JsonPropertyName("content")] public required string Content { get; init; } = content;
    }

    public sealed class Result { [JsonPropertyName("comment_id")] public string CommentId { get; init; } = string.Empty; }
}
