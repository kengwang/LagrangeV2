using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handler.File;

[Api("get_private_file_download_url")]
public class GetPrivateFileDownloadUrlHandler(BotContext bot) : IApiHandler<GetPrivateFileDownloadUrlParameter, GetPrivateFileDownloadUrlResult>
{
    private readonly BotContext _bot = bot;

    public async Task<GetPrivateFileDownloadUrlResult> HandleAsync(GetPrivateFileDownloadUrlParameter parameter, CancellationToken token)
    {
        return new GetPrivateFileDownloadUrlResult(await _bot.PrivateFSDownload(parameter.UserId, parameter.FileId, parameter.FileHash));
    }
}

public class GetPrivateFileDownloadUrlParameter(long userId, string fileId, string fileHash)
{
    [JsonRequired]
    [JsonPropertyName("user_id")]
    public long UserId { get; init; } = userId;

    [JsonRequired]
    [JsonPropertyName("file_id")]
    public string FileId { get; init; } = fileId;

    [JsonRequired]
    [JsonPropertyName("file_hash")]
    public string FileHash { get; init; } = fileHash;
}

public class GetPrivateFileDownloadUrlResult(string downloadUrl)
{
    [JsonPropertyName("download_url")]
    public string DownloadUrl { get; } = downloadUrl;
}
