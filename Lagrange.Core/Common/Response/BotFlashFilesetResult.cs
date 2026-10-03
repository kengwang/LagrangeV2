namespace Lagrange.Core.Common.Response;

public sealed class BotFlashFilesetResult
{
    public required IReadOnlyList<BotFlashFileEntry> Entries { get; init; }
}

public sealed class BotFlashFileEntry
{
    public required string FilesetUuid { get; init; }
    public required string FileName { get; init; }
    public required string OriginalName { get; init; }
    public uint FileType { get; init; }
    public ulong FileSize { get; init; }
    public string? UploadUrl { get; init; }
    public string? FileId { get; init; }
    public string? DownloadUrl { get; init; }
}
