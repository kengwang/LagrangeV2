namespace Lagrange.Core.Common.Response;

public sealed class BotQzoneUploadResult
{
    public required string RichValue { get; init; }
    public required string Url { get; init; }
    public required string AlbumId { get; init; }
    public required string Location { get; init; }
    public int Type { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
}
