using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.File;

[ApiHandler("set_flash_task_status")]
public sealed class SetFlashTaskStatusHandler(BotContext lagrange) : INoResultApiHandler<SetFlashTaskStatusHandler.Request>
{
    public async ValueTask<MilkyApiResponse> HandleAsync(Request request, CancellationToken ct)
    {
        await lagrange.SetFlashTaskStatus(request.FilesetUuid, request.Status, ct).WaitAsync(ct);
        return new MilkyApiResponse();
    }
    public sealed class Request(string filesetUuid, uint status = 6)
    {
        [JsonPropertyName("fileset_uuid")] public required string FilesetUuid { get; init; } = filesetUuid;
        [JsonPropertyName("status")] public uint Status { get; init; } = status;
    }
}
