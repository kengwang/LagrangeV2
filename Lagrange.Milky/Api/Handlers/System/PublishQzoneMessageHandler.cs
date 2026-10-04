using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.System;

public sealed class PublishQzoneMessageHandler(BotContext lagrange) : Endpoint<PublishQzoneMessageHandler.Request, MilkyApiResponse<PublishQzoneMessageHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/publish_qzone_message");
    }
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        var result = await lagrange.PublishQzoneMessage(request.Content, request.RichValue, ct).WaitAsync(ct);
        return new(new Result { MessageId = result.MessageId, Time = result.Time });
    }

    public sealed class Request(string content, string richValue = "")
    {
        [JsonPropertyName("content")] public required string Content { get; init; } = content;
        [JsonPropertyName("rich_value")] public string RichValue { get; init; } = richValue;
    }

    public sealed class Result
    {
        [JsonPropertyName("message_id")] public string MessageId { get; init; } = string.Empty;
        [JsonPropertyName("time")] public long Time { get; init; }
    }
}
