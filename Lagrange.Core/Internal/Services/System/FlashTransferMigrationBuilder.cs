using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Utility;
using System.Security.Cryptography;

namespace Lagrange.Core.Internal.Services.System;

internal static class FlashTransferMigrationBuilder
{
    private static int _sequence;
    internal static MigrationFlashApplyHead Head(uint sub, string? thumbnail = null) => new()
    {
        Sub = new() { Seq = (uint)Interlocked.Increment(ref _sequence), Sub = sub },
        Config = new() { Field101 = 2, Field102 = 4, Field103 = thumbnail is null ? 22u : thumbnail == "jpg" ? 24u : 23u, Field200 = 5 },
        Field3 = new() { Field1 = 1 }
    };

    internal static MigrationFlashApplyFileInfo Info(string name, uint size, string sha1, string md5, string? thumbnail) => new()
    {
        FileName = name, FileSize = size, Sha1 = sha1, Md5 = md5,
        Field5 = new() { Field1 = thumbnail == "jpg" ? 1u : 0u },
        Field6 = thumbnail is null ? 0u : 526u, Field7 = thumbnail is null ? 0u : 360u,
        Field9 = thumbnail == "jpg" ? 0u : 1u
    };

    internal static MigrationFlashApplyFilesetWrap Wrap(string set, string file, uint index, uint format, string? thumbnail, bool prepare) => new()
    {
        FilesetUuid = set, UploadKey = set, FileUuid = file, Field4 = index,
        Field5 = !prepare && thumbnail == "png" ? 1u : 0u,
        Field6 = thumbnail is not null && (prepare || thumbnail == "jpg") ? 1u : 0u,
        Field7 = format, Field8 = new(), Field9 = 1
    };

    internal static MigrationFlashPrepareUploadReq Prepare(string set, string file, uint index, uint format, string name, uint size, string sha1, string? thumbnail) => new()
    {
        Head = Head(100, thumbnail), Payload = new()
        {
            Wrapper = new() { FileInfo = Info(name, size, sha1, "", thumbnail) }, Field2 = 1,
            Field6 = new() { Field1 = new() { Field2 = new() }, Field2 = new() { Field3 = new() }, Field3 = new() { Field11 = new(), Field12 = new() } },
            FilesetWrap = Wrap(set, file, index, format, thumbnail, true)
        }
    };

    internal static MigrationFlashApplyUploadReq Apply(string set, string file, uint index, uint format, string name, uint size, string sha1, string md5, string fileId, string? thumbnail) => new()
    {
        Head = Head(103, thumbnail), Payload = new()
        {
            Wrapper = new() { FileInfo = Info(name, size, sha1, md5, thumbnail), FileId = fileId, Field3 = 1, Field4 = checked((uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds()), Field5 = 1209600 },
            Flag2 = new() { Field1 = 2 }, Field3 = new() { Field4 = new() },
            FilesetWrap = Wrap(set, file, index, format, thumbnail, false)
        }
    };

    internal static string FileId(byte[] sha1, uint size, uint appId) => Convert.ToBase64String(ProtoHelper.Serialize(new MigrationFlashFileId
    {
        Sha1 = sha1, FileSize = size, Appid = appId, Timestamp = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 1000,
        Env = "prod", Ttl = 1209600, SessionId = RandomNumberGenerator.GetBytes(16), Field15 = RandomNumberGenerator.GetBytes(3), Region = "gz"
    }).Span).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    internal static MigrationFlashGetDownloadUrlReq Metadata(string set) => new()
    {
        FilesetUuid = set, Field3 = 7, Field4 = 1,
        Inner = new() { Field2 = 1, Field3 = 18, Field5 = new(), Field6 = new() }
    };

    internal static MigrationFlashGetDownloadReq Download(MigrationFlashDownloadFileInfo file) => new()
    {
        Head = Head(200), Payload = new()
        {
            Wrapper = new() { FileId = file.MainFile!.FileId, FileInfo = new() { Field4 = file.FileName, Field5 = new() } },
            Field2 = new()
            {
                Field2 = new() { Field1 = 4294967294, Field3 = uint.MaxValue, Field5 = 111, Field6 = new() { Field1 = 3403722988 } },
                Field4 = new(), FilesetWrap = new() { FilesetUuid = file.FilesetUuid, FileUuid = file.FileUuid, Field3 = 26, Field4 = file.FileUuid }
            }
        }
    };

    internal static uint Format(string name) => Path.GetExtension(name).ToLowerInvariant() switch
    {
        ".png" or ".jpg" or ".jpeg" or ".gif" or ".bmp" or ".webp" or ".heic" or ".avif" or ".tiff" or ".tif" or ".ico" or ".dib" or ".heif" => 26,
        ".mp3" or ".wav" or ".aac" or ".flac" => 1,
        ".mp4" or ".avi" or ".mkv" or ".mov" or ".3gp" or ".mpeg" or ".rmvb" or ".rm" or ".wmv" or ".flv" or ".asf" or ".webm" or ".mpg" or ".vob" or ".m4v" or ".f4v" => 2,
        ".doc" or ".docx" => 3, ".zip" or ".rar" or ".tar" or ".bz2" or ".xz" or ".tgz" or ".gz" or ".7z" => 4,
        ".apk" => 5, ".xls" or ".xlsx" => 6, ".ppt" or ".pptx" => 7, ".html" or ".htm" => 8, ".pdf" => 9, ".txt" => 10,
        ".psd" => 12, ".pt" or ".pth" or ".onnx" or ".model" or ".mlmodel" => 15, ".ttf" or ".otf" => 16,
        ".ipa" => 17, ".key" => 18, ".note" => 19, ".numbers" => 20, ".pages" => 21, ".sketch" => 22, ".dmg" => 23, ".pkg" => 24, ".exe" => 27, _ => 11
    };
}
