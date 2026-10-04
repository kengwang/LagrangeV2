namespace Lagrange.Core.Common.Response;

/// <summary>Server-confirmed group album video and cover information.</summary>
public sealed class BotGroupAlbumVideoUploadResult
{
    /// <summary>The video id returned by the video upload.</summary>
    public required string VideoId { get; init; }
    /// <summary>The destination album id.</summary>
    public required string AlbumId { get; init; }
    /// <summary>The cover photo id returned by the cover upload.</summary>
    public required string CoverId { get; init; }
    /// <summary>The cover URL returned by the cover upload.</summary>
    public required string CoverUrl { get; init; }
}
