using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Converters;

namespace Lagrange.Milky.Api.Handlers.File;

public sealed class UploadPrivateFileHandler(BotContext lagrange, ResourceConverter resourceConverter) : Endpoint<UploadPrivateFileHandler.Request, MilkyApiResponse<UploadPrivateFileHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/upload_private_file");
    }
    private readonly BotContext _lagrange = lagrange;
    private readonly ResourceConverter _resourceConverter = resourceConverter;

    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        using var stream = await _resourceConverter.UriToStreamAsync(request.FileUri, ct);
        (ulong sequence, long time) = await _lagrange.SendFriendFile(
            request.UserId,
            stream,
            request.FileName,
            ct
        ).WaitAsync(ct);
        return new MilkyApiResponse<Result>(new Result
        {
            FileId = $"{sequence}_{time}"
        });
    }

    public sealed class Request(long userId, string fileUri, string fileName)
    {
        [JsonPropertyName("user_id")] public required long UserId { get; init; } = userId;
        [JsonPropertyName("file_uri")] public required string FileUri { get; init; } = fileUri;
        [JsonPropertyName("file_name")] public required string FileName { get; init; } = fileName;
    }

    public sealed class Result
    {
        [JsonPropertyName("file_id")] public required string FileId { get; init; }
    }
}
