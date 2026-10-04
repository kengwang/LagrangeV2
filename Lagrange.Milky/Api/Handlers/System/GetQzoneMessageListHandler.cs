using FastEndpoints;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Common.Response;

namespace Lagrange.Milky.Api.Handlers.System;

public sealed class GetQzoneMessageListHandler(BotContext lagrange) : Endpoint<GetQzoneMessageListHandler.Request, MilkyApiResponse<GetQzoneMessageListHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/get_qzone_msg_list");
    }
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        var result = await lagrange.GetQzoneMessageList(request.TargetUin, request.Position, request.Count, ct).WaitAsync(ct);
        return new(new Result(result));
    }

    public sealed class Request(long? targetUin = null, int position = 0, int count = 20)
    {
        [JsonPropertyName("target_uin")] public long? TargetUin { get; init; } = targetUin;
        [JsonPropertyName("position")] public int Position { get; init; } = position;
        [JsonPropertyName("count")] public int Count { get; init; } = count;
    }

    public sealed class Result(BotQzoneMessageListResult result)
    {
        [JsonPropertyName("total")] public int Total { get; } = result.Total;
        [JsonPropertyName("messages")] public Message[] Messages { get; } = result.Messages.Select(x => new Message(x)).ToArray();
    }

    public sealed class Message(BotQzoneMessage message)
    {
        [JsonPropertyName("id")] public string Id { get; } = message.Id;
        [JsonPropertyName("content")] public string Content { get; } = message.Content;
        [JsonPropertyName("time")] public long Time { get; } = message.Time;
        [JsonPropertyName("comment_count")] public int CommentCount { get; } = message.CommentCount;
        [JsonPropertyName("is_private")] public bool IsPrivate { get; } = message.IsPrivate;
        [JsonPropertyName("images")] public string[] Images { get; } = message.Images.ToArray();
    }
}
