using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.System;

[ApiHandler("delete_qzone_message")]
public sealed class DeleteQzoneMessageHandler(BotContext lagrange) : INoResultApiHandler<DeleteQzoneMessageHandler.Request>
{
    public async ValueTask<MilkyApiResponse> HandleAsync(Request request, CancellationToken ct)
    {
        await lagrange.DeleteQzoneMessage(request.MessageId, ct).WaitAsync(ct);
        return new MilkyApiResponse();
    }

    public sealed class Request(string messageId)
    {
        [JsonPropertyName("message_id")] public required string MessageId { get; init; } = messageId;
    }
}
