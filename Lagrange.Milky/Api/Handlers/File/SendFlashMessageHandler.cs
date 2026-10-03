using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.File;

[ApiHandler("send_flash_message")]
public sealed class SendFlashMessageHandler(BotContext lagrange) : INoResultApiHandler<SendFlashMessageHandler.Request>
{
    public async ValueTask<MilkyApiResponse> HandleAsync(Request request, CancellationToken ct)
    {
        await lagrange.SendFlashMessage(request.UserId, request.GroupId, request.FilesetUuid, ct).WaitAsync(ct);
        return new MilkyApiResponse();
    }
    public sealed class Request(string filesetUuid, long? userId, long? groupId)
    {
        [JsonPropertyName("fileset_uuid")] public required string FilesetUuid { get; init; } = filesetUuid;
        [JsonPropertyName("user_id")] public long? UserId { get; init; } = userId;
        [JsonPropertyName("group_id")] public long? GroupId { get; init; } = groupId;
    }
}
