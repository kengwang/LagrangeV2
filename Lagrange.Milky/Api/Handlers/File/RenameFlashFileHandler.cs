using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.File;

public sealed class RenameFlashFileHandler(BotContext lagrange) : Endpoint<RenameFlashFileHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/rename_flash_file");
    }
    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
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
