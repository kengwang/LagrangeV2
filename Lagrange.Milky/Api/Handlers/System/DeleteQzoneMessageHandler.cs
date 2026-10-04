using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.System;

public sealed class DeleteQzoneMessageHandler(BotContext lagrange) : Endpoint<DeleteQzoneMessageHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/delete_qzone_message");
    }
    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
    {
        await lagrange.DeleteQzoneMessage(request.MessageId, ct).WaitAsync(ct);
        return new MilkyApiResponse();
    }

    public sealed class Request(string messageId)
    {
        [JsonPropertyName("message_id")] public required string MessageId { get; init; } = messageId;
    }
}
