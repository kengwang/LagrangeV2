using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Converters;

namespace Lagrange.Milky.Api.Handlers.File;

/// <summary>Uploads a complete QQ flash fileset.</summary>
public sealed class UploadFlashFilesHandler(BotContext lagrange, ResourceConverter resources) : Endpoint<UploadFlashFilesHandler.Request, MilkyApiResponse<UploadFlashFilesHandler.Result>>
{
    /// <inheritdoc />
    public override void Configure() { AuthSchemes("Milky"); Post("/api/upload_flash_files"); }
    /// <inheritdoc />
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request.Files);
        var streams = new List<Stream>();
        try
        {
            var files = new List<FlashTransferFile>();
            foreach (var item in request.Files)
            {
                var stream = await resources.UriToStreamAsync(item.FileUri, ct);
                streams.Add(stream);
                files.Add(new(item.FileName, stream));
            }
            var result = await lagrange.UploadFlashFiles(files, request.Name, ct);
            return new(new Result { FilesetUuid = result.FilesetUuid, ShareUrl = result.ShareUrl, Files = result.Files.Select(x => new FileResult { FileUuid = x.FileUuid, FileIndex = x.FileIndex, FileName = x.FileName, FileSize = x.FileSize }).ToList() });
        }
        finally { foreach (var stream in streams) await stream.DisposeAsync(); }
    }
    /// <summary>Upload inputs.</summary>
    public sealed class Request
    {
        /// <summary>Named resource URIs.</summary>
        [JsonPropertyName("files")] public required List<FileInput> Files { get; init; }
        /// <summary>Optional fileset display title.</summary>
        [JsonPropertyName("name")] public string? Name { get; init; }
    }
    /// <summary>One file source.</summary>
    public sealed class FileInput
    {
        /// <summary>Resource URI.</summary>
        [JsonPropertyName("file_uri")] public required string FileUri { get; init; }
        /// <summary>Display filename.</summary>
        [JsonPropertyName("file_name")] public required string FileName { get; init; }
    }
    /// <summary>Completed upload.</summary>
    public sealed class Result
    {
        /// <summary>Fileset identifier.</summary>
        [JsonPropertyName("fileset_uuid")] public required string FilesetUuid { get; init; }
        /// <summary>Official sharing URL.</summary>
        [JsonPropertyName("share_url")] public string? ShareUrl { get; init; }
        /// <summary>Primary file identities.</summary>
        [JsonPropertyName("files")] public required List<FileResult> Files { get; init; }
    }
    /// <summary>Uploaded primary file.</summary>
    public sealed class FileResult
    {
        /// <summary>File identifier.</summary>
        [JsonPropertyName("file_uuid")] public required string FileUuid { get; init; }
        /// <summary>One-based index.</summary>
        [JsonPropertyName("file_index")] public uint FileIndex { get; init; }
        /// <summary>Filename.</summary>
        [JsonPropertyName("file_name")] public required string FileName { get; init; }
        /// <summary>Size in bytes.</summary>
        [JsonPropertyName("file_size")] public ulong FileSize { get; init; }
    }
}

/// <summary>Fetches primary-file download metadata without downloading content.</summary>
public sealed class GetFlashDownloadHandler(BotContext lagrange) : Endpoint<GetFlashDownloadHandler.Request, MilkyApiResponse<GetFlashDownloadHandler.Result>>
{
    /// <inheritdoc />
    public override void Configure() { AuthSchemes("Milky"); Post("/api/get_flash_download"); }
    /// <inheritdoc />
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        var result = await lagrange.GetFlashDownload(request.FilesetUuid, request.FileIndex ?? 1, ct);
        return new(new Result { DownloadUrl = result.Url, FileName = result.FileName, FileSize = result.FileSize });
    }
    /// <summary>File selection.</summary>
    public sealed class Request
    {
        /// <summary>Fileset identifier.</summary>
        [JsonPropertyName("fileset_uuid")] public required string FilesetUuid { get; init; }
        /// <summary>One-based primary file index.</summary>
        [JsonPropertyName("file_index")] public uint? FileIndex { get; init; }
    }
    /// <summary>Download descriptor.</summary>
    public sealed class Result
    {
        /// <summary>Main file URL.</summary>
        [JsonPropertyName("download_url")] public required string DownloadUrl { get; init; }
        /// <summary>Filename.</summary>
        [JsonPropertyName("file_name")] public required string FileName { get; init; }
        /// <summary>File size in bytes.</summary>
        [JsonPropertyName("file_size")] public ulong FileSize { get; init; }
    }
}

/// <summary>Fetches the official sharing link for a fileset.</summary>
public sealed class GetFlashShareLinkHandler(BotContext lagrange) : Endpoint<GetFlashShareLinkHandler.Request, MilkyApiResponse<GetFlashShareLinkHandler.Result>>
{
    /// <inheritdoc />
    public override void Configure() { AuthSchemes("Milky"); Post("/api/get_flash_share_link"); }
    /// <inheritdoc />
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct) => new(new Result { ShareUrl = await lagrange.GetFlashShareLink(request.FilesetUuid, ct) });
    /// <summary>Fileset selection.</summary>
    public sealed class Request
    {
        /// <summary>Fileset identifier.</summary>
        [JsonPropertyName("fileset_uuid")] public required string FilesetUuid { get; init; }
    }
    /// <summary>Share link.</summary>
    public sealed class Result
    {
        /// <summary>Official sharing URL.</summary>
        [JsonPropertyName("share_url")] public required string ShareUrl { get; init; }
    }
}

/// <summary>Resolves an official QQ share code to a fileset.</summary>
public sealed class ResolveFlashShareCodeHandler(BotContext lagrange) : Endpoint<ResolveFlashShareCodeHandler.Request, MilkyApiResponse<ResolveFlashShareCodeHandler.Result>>
{
    /// <inheritdoc />
    public override void Configure() { AuthSchemes("Milky"); Post("/api/resolve_flash_share_code"); }
    /// <inheritdoc />
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct) => new(new Result { FilesetUuid = await lagrange.ResolveFlashShareCode(request.ShareCode, ct) });
    /// <summary>Share code input.</summary>
    public sealed class Request
    {
        /// <summary>Official URL or bare share code.</summary>
        [JsonPropertyName("share_code")] public required string ShareCode { get; init; }
    }
    /// <summary>Resolved identifier.</summary>
    public sealed class Result
    {
        /// <summary>Fileset UUID.</summary>
        [JsonPropertyName("fileset_uuid")] public required string FilesetUuid { get; init; }
    }
}
