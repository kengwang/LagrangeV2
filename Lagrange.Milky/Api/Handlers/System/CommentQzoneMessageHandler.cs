using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.System;

[ApiHandler("comment_qzone_message")]
public sealed class CommentQzoneMessageHandler(BotContext lagrange) : IApiHandler<CommentQzoneMessageHandler.Request, CommentQzoneMessageHandler.Result>
{
    public async ValueTask<MilkyApiResponse<Result>> HandleAsync(Request request, CancellationToken ct)
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
