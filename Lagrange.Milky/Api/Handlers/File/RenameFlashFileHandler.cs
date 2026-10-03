using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.File;

[ApiHandler("rename_flash_file")]
public sealed class RenameFlashFileHandler(BotContext lagrange) : INoResultApiHandler<RenameFlashFileHandler.Request>
{
    public async ValueTask<MilkyApiResponse> HandleAsync(Request request, CancellationToken ct)
    {
        await lagrange.RenameFlashFile(request.FilesetUuid, request.NewName, ct).WaitAsync(ct);
        return new MilkyApiResponse();
    }
    public sealed class Request(string filesetUuid, string newName)
    {
        [JsonPropertyName("fileset_uuid")] public required string FilesetUuid { get; init; } = filesetUuid;
        [JsonPropertyName("new_name")] public required string NewName { get; init; } = newName;
    }
}
