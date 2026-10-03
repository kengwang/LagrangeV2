using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.File;

[ApiHandler("complete_flash_task")]
public sealed class CompleteFlashTaskHandler(BotContext lagrange) : INoResultApiHandler<CompleteFlashTaskHandler.Request>
{
    public async ValueTask<MilkyApiResponse> HandleAsync(Request request, CancellationToken ct)
    {
        await lagrange.CompleteFlashTask(request.FilesetUuid, ct).WaitAsync(ct);
        return new MilkyApiResponse();
    }
    public sealed class Request(string filesetUuid) { [JsonPropertyName("fileset_uuid")] public required string FilesetUuid { get; init; } = filesetUuid; }
}
