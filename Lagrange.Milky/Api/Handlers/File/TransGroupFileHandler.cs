using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.File;

[ApiHandler("trans_group_file")]
public sealed class TransGroupFileHandler(BotContext lagrange) : IApiHandler<TransGroupFileHandler.Request, TransGroupFileHandler.Result>
{
    private readonly BotContext _lagrange = lagrange;

    public async ValueTask<MilkyApiResponse<Result>> HandleAsync(Request request, CancellationToken ct)
    {
        var result = await _lagrange.TransferGroupFile(request.GroupId, request.FileId, ct);
        return new MilkyApiResponse<Result>(new Result { SaveBusId = result.SaveBusId, SaveFilePath = result.SaveFilePath });
    }

    public sealed class Request(long groupId, string fileId)
    {
        [JsonPropertyName("group_id")] public required long GroupId { get; init; } = groupId;
        [JsonPropertyName("file_id")] public required string FileId { get; init; } = fileId;
    }

    public sealed class Result
    {
        [JsonPropertyName("save_bus_id")] public int SaveBusId { get; init; }
        [JsonPropertyName("save_file_path")] public required string SaveFilePath { get; init; }
    }
}
