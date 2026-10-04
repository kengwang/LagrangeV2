using FastEndpoints;
using System;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.File;

public sealed class GetPrivateFileDownloadUrlHandler(BotContext lagrange) : Endpoint<GetPrivateFileDownloadUrlHandler.Request, MilkyApiResponse<GetPrivateFileDownloadUrlHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/get_private_file_download_url");
    }
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.FileId);
        string url = await lagrange.GetNTV2RichMediaUrl(request.FileId).WaitAsync(ct);
        return new(new Result { DownloadUrl = url });
    }
    public sealed class Request(string fileId) { [JsonPropertyName("file_id")] public required string FileId { get; init; } = fileId; }
    public sealed class Result { [JsonPropertyName("download_url")] public required string DownloadUrl { get; init; } }
}
