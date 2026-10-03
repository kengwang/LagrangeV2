namespace Lagrange.Core.Common.Response;

public sealed class BotGroupAlbumResult
{
    public required IReadOnlyList<BotGroupAlbum> Albums { get; init; }
    public bool HasMore { get; init; }
    public string AttachInfo { get; init; } = string.Empty;
}

public sealed class BotGroupAlbum
{
    public required string AlbumId { get; init; }
    public string Owner { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public ulong CreateTime { get; init; }
    public ulong ModifyTime { get; init; }
    public ulong LastUploadTime { get; init; }
    public ulong UploadNumber { get; init; }
    public string? CoverUrl { get; init; }
}

public sealed class BotGroupAlbumMediaResult
{
    public required IReadOnlyList<BotGroupAlbumMedia> Media { get; init; }
    public string PreviousCursor { get; init; } = string.Empty;
    public string NextCursor { get; init; } = string.Empty;
}

public sealed class BotGroupAlbumMedia
{
    public string Type { get; init; } = string.Empty;
    public string? Id { get; init; }
    public string? Url { get; init; }
    public string? CoverUrl { get; init; }
    public uint Width { get; init; }
    public uint Height { get; init; }
    public ulong UploadTime { get; init; }
}
