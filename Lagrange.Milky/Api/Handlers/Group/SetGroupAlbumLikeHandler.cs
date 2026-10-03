using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.Group;

[ApiHandler("set_group_album_like")]
public sealed class SetGroupAlbumLikeHandler(BotContext lagrange) : INoResultApiHandler<SetGroupAlbumLikeHandler.Request>
{
    public async ValueTask<MilkyApiResponse> HandleAsync(Request request, CancellationToken ct)
    {
        await lagrange.SetGroupAlbumLike(request.GroupId, request.AlbumId, request.BatchId, request.MediaId, request.IsLike, ct).WaitAsync(ct);
        return new MilkyApiResponse();
    }
    public sealed class Request(long groupId, string albumId, string batchId, string? mediaId = null, bool isLike = true)
    {
        [JsonPropertyName("group_id")] public long GroupId { get; init; } = groupId;
        [JsonPropertyName("album_id")] public required string AlbumId { get; init; } = albumId;
        [JsonPropertyName("batch_id")] public required string BatchId { get; init; } = batchId;
        [JsonPropertyName("media_id")] public string? MediaId { get; init; } = mediaId;
        [JsonPropertyName("is_like")] public bool IsLike { get; init; } = isLike;
    }
}
