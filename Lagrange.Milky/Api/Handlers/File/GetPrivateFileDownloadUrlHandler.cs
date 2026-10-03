using System;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.File;

[ApiHandler("get_private_file_download_url")]
public sealed class GetPrivateFileDownloadUrlHandler(BotContext lagrange) : IApiHandler<GetPrivateFileDownloadUrlHandler.Request, GetPrivateFileDownloadUrlHandler.Result>
{
    public async ValueTask<MilkyApiResponse<Result>> HandleAsync(Request request, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.FileId);
        var url = await lagrange.GetNTV2RichMediaUrl(request.FileId).WaitAsync(ct);
        return new(new Result { DownloadUrl = url });
    }
    public sealed class Request(string fileId) { [JsonPropertyName("file_id")] public required string FileId { get; init; } = fileId; }
    public sealed class Result { [JsonPropertyName("download_url")] public required string DownloadUrl { get; init; } }
}
