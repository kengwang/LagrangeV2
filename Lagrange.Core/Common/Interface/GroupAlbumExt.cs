using Lagrange.Core.Common.Response;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;

namespace Lagrange.Core.Common.Interface;

/// <summary>Additional group album operations.</summary>
public static class GroupAlbumExt
{
    /// <summary>Reads every album page, rejecting missing or repeated continuation cursors.</summary>
    /// <param name="context">The logged-in bot.</param>
    /// <param name="groupUin">Group number.</param>
    /// <param name="cancellationToken">Cancels pagination.</param>
    /// <returns>All albums returned by the server.</returns>
    public static Task<IReadOnlyList<BotGroupAlbum>> GetAllGroupAlbums(this BotContext context, long groupUin, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return ReadAllPages((cursor, ct) => context.GetGroupAlbums(groupUin, cursor, ct), cancellationToken);
    }

    internal static async Task<IReadOnlyList<BotGroupAlbum>> ReadAllPages(Func<string, CancellationToken, Task<BotGroupAlbumResult>> fetch, CancellationToken ct)
    {
        List<BotGroupAlbum> albums = [];
        HashSet<string> cursors = [string.Empty];
        string cursor = string.Empty;
        while (true)
        {
            ct.ThrowIfCancellationRequested();
            var page = await fetch(cursor, ct).ConfigureAwait(false);
            ValidatePage(page, cursor);
            albums.AddRange(page.Albums);
            if (!page.HasMore) return albums;
            if (!cursors.Add(page.AttachInfo)) throw new OperationException(-1, "Group album pagination returned a repeated cursor.");
            cursor = page.AttachInfo;
        }
    }

    internal static void ValidatePage(BotGroupAlbumResult page, string cursor)
    {
        if (page.HasMore && (string.IsNullOrEmpty(page.AttachInfo) || page.AttachInfo == cursor))
            throw new OperationException(-1, "Group album pagination did not advance its cursor.");
    }

    /// <summary>Uploads a video and its cover to an existing group album.</summary>
    /// <remarks>Streams remain owned by the caller. Seekable inputs are read from their beginning and their positions are restored. Non-seekable inputs are staged temporarily and cleaned up on completion.</remarks>
    /// <param name="context">The logged-in bot.</param>
    /// <param name="groupUin">Group number.</param>
    /// <param name="albumId">Existing album id.</param>
    /// <param name="albumName">Existing album name.</param>
    /// <param name="video">An ISO BMFF video, such as MP4.</param>
    /// <param name="cover">The actual video cover image.</param>
    /// <param name="durationMilliseconds">The video's measured playback duration, in milliseconds.</param>
    /// <param name="fileName">Video display name.</param>
    /// <param name="cancellationToken">Cancels pending requests and upload reads.</param>
    /// <returns>The server's video id and cover metadata after both uploads finish.</returns>
    public static async Task<BotGroupAlbumVideoUploadResult> UploadGroupAlbumVideo(this BotContext context, long groupUin, string albumId,
        string albumName, Stream video, Stream cover, long durationMilliseconds, string fileName = "video.mp4", CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return (await context.EventContext.SendEvent<UploadGroupAlbumVideoEventResp>(
            new UploadGroupAlbumVideoEventReq(groupUin, albumId, albumName, video, cover, durationMilliseconds, fileName), cancellationToken).ConfigureAwait(false)).Result;
    }
}
