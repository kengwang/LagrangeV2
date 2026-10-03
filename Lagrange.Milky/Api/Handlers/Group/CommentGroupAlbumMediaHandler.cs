using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.Group;

[ApiHandler("comment_group_album_media")]
public sealed class CommentGroupAlbumMediaHandler(BotContext lagrange) : IApiHandler<CommentGroupAlbumMediaHandler.Request, CommentGroupAlbumMediaHandler.Result>
{
    public async ValueTask<MilkyApiResponse<Result>> HandleAsync(Request request, CancellationToken ct)
    {
        var id = await lagrange.CommentGroupAlbumMedia(request.GroupId, request.AlbumId, request.MediaId, request.Content, ct).WaitAsync(ct);
        return new(new Result { CommentId = id });
    }
    public sealed class Request(long groupId, string albumId, string mediaId, string content)
    {
        [JsonPropertyName("group_id")] public long GroupId { get; init; } = groupId;
        [JsonPropertyName("album_id")] public required string AlbumId { get; init; } = albumId;
        [JsonPropertyName("media_id")] public required string MediaId { get; init; } = mediaId;
        [JsonPropertyName("content")] public required string Content { get; init; } = content;
    }
    public sealed class Result { [JsonPropertyName("comment_id")] public string CommentId { get; init; } = string.Empty; }
}
