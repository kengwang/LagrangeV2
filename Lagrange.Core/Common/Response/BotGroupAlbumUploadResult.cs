namespace Lagrange.Core.Common.Response;

public sealed class BotGroupAlbumUploadResult
{
    public required string MediaId { get; init; }
    public required string AlbumId { get; init; }
    public required string Url { get; init; }
}
