using System.Buffers.Binary;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Events;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Internal.Services.System;

namespace Lagrange.Core.Common.Interface;

/// <summary>A named stream to copy into a flash fileset. The caller retains stream ownership.</summary>
/// <param name="FileName">The display filename.</param>
/// <param name="Content">Readable content, copied from its current position.</param>
public sealed record FlashTransferFile(string FileName, Stream Content);
/// <summary>An uploaded file's stable identity within a fileset.</summary>
/// <param name="FileUuid">File UUID.</param><param name="FileIndex">One-based index.</param><param name="FileName">Display name.</param><param name="FileSize">Length in bytes.</param>
public sealed record FlashTransferUploadedFile(string FileUuid, uint FileIndex, string FileName, ulong FileSize);
/// <summary>A successfully uploaded and finalized fileset.</summary>
/// <param name="FilesetUuid">Fileset identifier.</param><param name="ShareUrl">Official share URL.</param><param name="Files">Uploaded primary files.</param>
public sealed record FlashTransferUploadResult(string FilesetUuid, string? ShareUrl, IReadOnlyList<FlashTransferUploadedFile> Files);
/// <summary>A download descriptor; no file content is downloaded.</summary>
/// <param name="Url">Main-file download URL.</param><param name="FileName">Filename.</param><param name="FileSize">Size in bytes.</param>
public sealed record FlashTransferDownload(string Url, string FileName, ulong FileSize);

/// <summary>QQ flash transfer extensions, including upload orchestration and official share resolution.</summary>
public static partial class FlashTransferExt
{
    /// <summary>Stages and uploads all streams as one fileset. A failure throws without claiming completion.</summary>
    /// <param name="context">Bot session.</param><param name="files">Files to upload.</param><param name="name">Optional fileset title.</param><param name="cancellationToken">Cancellation.</param>
    /// <returns>The finalized fileset and individual primary file identities.</returns>
    public static Task<FlashTransferUploadResult> UploadFlashFiles(this BotContext context, IReadOnlyList<FlashTransferFile> files, string? name = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return UploadCore(new SessionTransport(context), files, name, cancellationToken);
    }

    internal static async Task<FlashTransferUploadResult> UploadCore(IFlashTransferTransport transport, IReadOnlyList<FlashTransferFile> files, string? name, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(files);
        if (files.Count == 0) throw new ArgumentException("At least one file is required.", nameof(files));
        var staged = new List<(FlashTransferUploadedFile Info, FileStream Stream)>();
        try
        {
            foreach (var file in files)
            {
                ArgumentNullException.ThrowIfNull(file);
                ArgumentException.ThrowIfNullOrWhiteSpace(file.FileName);
                ArgumentNullException.ThrowIfNull(file.Content);
                string cleanName = file.FileName.Replace('/', '_').Replace('\\', '_').Trim();
                var stream = new FileStream(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()), FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, 65536, FileOptions.Asynchronous | FileOptions.DeleteOnClose);
                try
                {
                    byte[] buffer = new byte[65536];
                    int count;
                    while ((count = await file.Content.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) != 0)
                    {
                        if (stream.Length + count > uint.MaxValue) throw new ArgumentOutOfRangeException(nameof(files), "Flash files must be smaller than 4 GiB (32-bit wire size).");
                        await stream.WriteAsync(buffer.AsMemory(0, count), cancellationToken).ConfigureAwait(false);
                    }
                    if (stream.Length == 0) throw new ArgumentException("Flash files must not be empty.", nameof(files));
                    staged.Add((new(Guid.NewGuid().ToString(), (uint)staged.Count + 1, cleanName, (ulong)stream.Length), stream));
                }
                catch { await stream.DisposeAsync().ConfigureAwait(false); throw; }
            }
            var first = staged[0].Info;
            string title = string.IsNullOrWhiteSpace(name) ? (staged.Count == 1 ? first.FileName : $"{first.FileName}等{staged.Count}个文件") : name.Trim();
            uint type = Path.GetExtension(first.FileName).Equals(".zip", StringComparison.OrdinalIgnoreCase) ? 6u : FlashTransferMigrationBuilder.Format(first.FileName) == 4 ? 2u : 7u;
            ulong total = staged.Aggregate(0UL, (sum, x) => checked(sum + x.Info.FileSize));
            BotFlashCreateResult created = (await transport.Send<CreateFlashTaskEventResp>(new CreateFlashTaskEventReq(title, total, type), cancellationToken).ConfigureAwait(false)).Result;
            var entries = staged.Select(x => new D93D0Info { FilesetUuid = created.FilesetUuid, FileUuid = x.Info.FileUuid, Index = x.Info.FileIndex, FormatCode = FlashTransferMigrationBuilder.Format(x.Info.FileName), FileName = x.Info.FileName, OriginalName = x.Info.FileName, FileSize = x.Info.FileSize }).ToList();
            await transport.Send<CommitFlashFileEventResp>(new CommitFlashFileEventReq(created.FilesetUuid, created.UploadKey, first.FileUuid, first.FileName, first.FileSize, 1, entries[0].FormatCode!.Value) { Entries = entries }, cancellationToken).ConfigureAwait(false);
            await transport.Send<CompleteFlashTaskEventResp>(new CompleteFlashTaskEventReq(created.FilesetUuid), cancellationToken).ConfigureAwait(false);
            var prepared = new List<(FileStream Stream, string Key)>();
            foreach (var (info, stream) in staged)
            {
                string? key = await Prepare(transport, created.FilesetUuid, info, stream, null, cancellationToken).ConfigureAwait(false);
                if (key is not null) prepared.Add((stream, key));
            }
            foreach (var (stream, key) in prepared)
                if (!await transport.Upload(key, 14901, stream, cancellationToken).ConfigureAwait(false)) throw new OperationException(-1, "Flash file slice upload was rejected.");
            foreach (string thumbnail in new[] { "png", "jpg" })
            {
                using var bytes = new MemoryStream(Thumbnail());
                string id = thumbnail == "jpg" ? first.FileUuid : Guid.NewGuid().ToString();
                var info = new FlashTransferUploadedFile(id, (uint)staged.Count + (thumbnail == "png" ? 1u : 2u), Guid.NewGuid().ToString("N") + "." + thumbnail, (ulong)bytes.Length);
                string? key = await Prepare(transport, created.FilesetUuid, info, bytes, thumbnail, cancellationToken).ConfigureAwait(false);
                if (key is not null && !await transport.Upload(key, thumbnail == "png" ? 14903u : 14902u, bytes, cancellationToken).ConfigureAwait(false)) throw new OperationException(-1, "Flash thumbnail upload was rejected.");
            }
            await transport.Send<SetFlashTaskStatusEventResp>(new SetFlashTaskStatusEventReq(created.FilesetUuid, 6), cancellationToken).ConfigureAwait(false);
            return new(created.FilesetUuid, created.UploadUrl, staged.Select(x => x.Info).ToArray());
        }
        finally { foreach (var (_, stream) in staged) await stream.DisposeAsync().ConfigureAwait(false); }
    }

    private static async Task<string?> Prepare(IFlashTransferTransport transport, string set, FlashTransferUploadedFile file, Stream stream, string? thumbnail, CancellationToken ct)
    {
        stream.Position = 0;
        byte[] sha = await SHA1.HashDataAsync(stream, ct).ConfigureAwait(false);
        stream.Position = 0;
        byte[] md5 = await MD5.HashDataAsync(stream, ct).ConfigureAwait(false);
        uint format = thumbnail == "jpg" ? 2u : FlashTransferMigrationBuilder.Format(file.FileName);
        uint size = checked((uint)file.FileSize);
        string hash = Convert.ToHexString(sha).ToLowerInvariant();
        var prepared = await transport.Send<FlashTransferPrepareResp>(new FlashTransferPrepareReq(FlashTransferMigrationBuilder.Prepare(set, file.FileUuid, file.FileIndex, format, file.FileName, size, hash, thumbnail)), ct).ConfigureAwait(false);
        string id = FlashTransferMigrationBuilder.FileId(sha, size, thumbnail == "png" ? 14903u : thumbnail == "jpg" ? 14902u : 14901u);
        await transport.Send<FlashTransferApplyResp>(new FlashTransferApplyReq(FlashTransferMigrationBuilder.Apply(set, file.FileUuid, file.FileIndex, format, file.FileName, size, hash, Convert.ToHexString(md5).ToLowerInvariant(), id, thumbnail)), ct).ConfigureAwait(false);
        return prepared.Body.RkeyWrap?.Rkey is { Length: > 0 } key ? key : null;
    }

    /// <summary>Resolves a primary file download, retrying metadata while a newly uploaded file is indexed.</summary>
    /// <param name="context">Bot session.</param><param name="filesetUuid">Fileset UUID.</param><param name="fileIndex">One-based index.</param><param name="cancellationToken">Cancellation.</param>
    /// <returns>Main-file URL, filename and size.</returns>
    public static Task<FlashTransferDownload> GetFlashDownload(this BotContext context, string filesetUuid, uint fileIndex = 1, CancellationToken cancellationToken = default) => Download(context, filesetUuid, fileIndex, null, cancellationToken);

    internal static async Task<FlashTransferDownload> Download(BotContext context, string filesetUuid, uint index, string? fileId, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filesetUuid);
        ArgumentOutOfRangeException.ThrowIfZero(index);
        int[] delays = [1000, 2000, 4000, 4000];
        for (int attempt = 0; ; attempt++)
        {
            var metadata = await context.EventContext.SendEvent<FlashTransferDownloadMetadataResp>(new FlashTransferDownloadMetadataReq(FlashTransferMigrationBuilder.Metadata(filesetUuid)), ct).ConfigureAwait(false);
            var file = metadata.Body.Entry?.FileInfo.Select((value, i) => (value, index: value.Field6 == 0 ? (uint)i + 1 : value.Field6)).FirstOrDefault(x => fileId is null ? x.index == index : x.value.MainFile?.FileId == fileId).value;
            if (file?.MainFile?.FileId is { Length: > 0 })
            {
                var download = await context.EventContext.SendEvent<FlashTransferDownloadResp>(new FlashTransferDownloadReq(FlashTransferMigrationBuilder.Download(file)), ct).ConfigureAwait(false);
                return new(ParseDownload(download.Body.Body), file.FileName, file.FileSize);
            }
            if (attempt == delays.Length) throw new OperationException(-1, "Flash primary file metadata is unavailable.");
            await Task.Delay(delays[attempt], ct).ConfigureAwait(false);
        }
    }

    internal static string ParseDownload(byte[] body)
    {
        string text = Encoding.Latin1.GetString(body);
        var path = DownloadPath().Match(text);
        if (!text.Contains("multimedia.qfile.qq.com", StringComparison.Ordinal) || !path.Success) throw new OperationException(-1, "Flash primary download URL is unavailable.");
        string url = "https://multimedia.qfile.qq.com" + path.Value;
        var key = DownloadKey().Match(text);
        return key.Success && !url.Contains("rkey=", StringComparison.Ordinal) ? url + "&" + key.Value : url;
    }

    /// <summary>Fetches the official sharing URL of a fileset.</summary>
    /// <param name="context">Bot session.</param><param name="filesetUuid">Fileset identifier.</param><param name="cancellationToken">Cancellation.</param><returns>The official share URL.</returns>
    public static async Task<string> GetFlashShareLink(this BotContext context, string filesetUuid, CancellationToken cancellationToken = default)
    {
        var result = await context.GetFlashFileset(filesetUuid, cancellationToken).ConfigureAwait(false);
        return result.Entries.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.UploadUrl))?.UploadUrl ?? throw new OperationException(-1, "Flash share URL is unavailable.");
    }

    /// <summary>Resolves an official qfile.qq.com share code or HTTPS URL without following redirects.</summary>
    /// <param name="context">Bot session.</param><param name="shareCode">Official URL or bare code.</param><param name="cancellationToken">Cancellation.</param><returns>Fileset UUID embedded in the official share page.</returns>
    public static async Task<string> ResolveFlashShareCode(this BotContext context, string shareCode, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        Uri uri = OfficialShareUri(shareCode);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(20));
        CancellationToken requestToken = timeout.Token;
        using var handler = new HttpClientHandler { AllowAutoRedirect = false };
        using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(20) };
        using var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, requestToken).ConfigureAwait(false);
        if ((int)response.StatusCode is >= 300 and < 400) throw new HttpRequestException("Flash share redirects are not allowed.");
        response.EnsureSuccessStatusCode();
        const int limit = 2 * 1024 * 1024;
        if (response.Content.Headers.ContentLength > limit) throw new InvalidDataException("Flash share page exceeds 2 MiB.");
        await using var input = await response.Content.ReadAsStreamAsync(requestToken).ConfigureAwait(false);
        using var output = new MemoryStream();
        byte[] buffer = new byte[8192]; int read;
        while ((read = await input.ReadAsync(buffer, requestToken).ConfigureAwait(false)) != 0)
        {
            if (output.Length + read > limit) throw new InvalidDataException("Flash share page exceeds 2 MiB.");
            output.Write(buffer, 0, read);
        }
        return ParseSharePage(Encoding.UTF8.GetString(output.GetBuffer(), 0, (int)output.Length));
    }

    internal static Uri OfficialShareUri(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        string code = value.Trim();
        if (code.Contains(':') || code.StartsWith("//", StringComparison.Ordinal))
        {
            if (!Uri.TryCreate(code, UriKind.Absolute, out var uri) || uri.Scheme != "https" || uri.Host != "qfile.qq.com" || !uri.IsDefaultPort || uri.Authority.Contains(':') || uri.UserInfo.Length != 0) throw new ArgumentException("Expected an official QQ share URL.", nameof(value));
            string path = uri.AbsolutePath.TrimEnd('/');
            if (!path.StartsWith("/q/", StringComparison.Ordinal)) throw new ArgumentException("Expected an official QQ share URL.", nameof(value));
            code = Uri.UnescapeDataString(path[3..]);
        }
        if (code.Length is 0 or > 256 || code.Any(c => c <= 32 || c == 127 || "/\\?#".Contains(c))) throw new ArgumentException("Invalid QQ share code.", nameof(value));
        return new Uri("https://qfile.qq.com/q/" + Uri.EscapeDataString(code));
    }

    internal static string ParseSharePage(string html)
    {
        var match = ShareIdentifier().Match(html);
        return match.Success ? match.Groups[1].Value : throw new InvalidDataException("Share page contains no fileset identifier.");
    }

    private static byte[] Thumbnail()
    {
        byte[] rgb = RandomNumberGenerator.GetBytes(3);
        byte[] pixels = new byte[(526 * 3 + 1) * 360];
        for (int y = 0; y < 360; y++) for (int x = 0; x < 526; x++) rgb.CopyTo(pixels, y * (526 * 3 + 1) + 1 + x * 3);
        using var compressed = new MemoryStream();
        using (var z = new ZLibStream(compressed, CompressionLevel.Fastest, true)) z.Write(pixels);
        using var output = new MemoryStream();
        output.Write([137, 80, 78, 71, 13, 10, 26, 10]);
        byte[] header = new byte[13]; BinaryPrimitives.WriteUInt32BigEndian(header, 526); BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(4), 360); header[8] = 8; header[9] = 2;
        WriteChunk(output, "IHDR", header); WriteChunk(output, "IDAT", compressed.ToArray()); WriteChunk(output, "IEND", []);
        return output.ToArray();
    }

    private static void WriteChunk(Stream output, string type, byte[] data)
    {
        Span<byte> number = stackalloc byte[4]; BinaryPrimitives.WriteUInt32BigEndian(number, (uint)data.Length); output.Write(number);
        byte[] tag = Encoding.ASCII.GetBytes(type); output.Write(tag); output.Write(data);
        uint crc = uint.MaxValue;
        foreach (byte b in tag.Concat(data)) { crc ^= b; for (int i = 0; i < 8; i++) crc = (crc >> 1) ^ ((crc & 1) == 0 ? 0 : 0xedb88320); }
        BinaryPrimitives.WriteUInt32BigEndian(number, ~crc); output.Write(number);
    }

    [GeneratedRegex(@"/download\?appid=14901[\x20-\x7e]*", RegexOptions.CultureInvariant)] private static partial Regex DownloadPath();
    [GeneratedRegex(@"rkey=[A-Za-z0-9_-]+", RegexOptions.CultureInvariant)] private static partial Regex DownloadKey();
    [GeneratedRegex("fileset_id\\\\?\"\\s*:\\s*\\\\?\"([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})", RegexOptions.CultureInvariant)] private static partial Regex ShareIdentifier();
}

internal interface IFlashTransferTransport
{
    Task<T> Send<T>(ProtocolEvent request, CancellationToken ct) where T : ProtocolEvent;
    Task<bool> Upload(string key, uint appId, Stream stream, CancellationToken ct);
}

internal sealed class SessionTransport(BotContext context) : IFlashTransferTransport
{
    public Task<T> Send<T>(ProtocolEvent request, CancellationToken ct) where T : ProtocolEvent => context.EventContext.SendEvent<T>(request, ct).AsTask();
    public Task<bool> Upload(string key, uint appId, Stream stream, CancellationToken ct) => context.FlashTransferContext.UploadFile(key, appId, stream, ct);
}
