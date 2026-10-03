using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.Group;

[ApiHandler("delete_group_album_media")]
public sealed class DeleteGroupAlbumMediaHandler(BotContext lagrange) : INoResultApiHandler<DeleteGroupAlbumMediaHandler.Request>
{
    public async ValueTask<MilkyApiResponse> HandleAsync(Request request, CancellationToken ct)
    {
        await lagrange.DeleteGroupAlbumMedia(request.GroupId, request.AlbumId, request.MediaId, request.BatchId, ct).WaitAsync(ct);
        return new MilkyApiResponse();
    }
    public sealed class Request(long groupId, string albumId, string mediaId, string? batchId = null)
    {
        [JsonPropertyName("group_id")] public long GroupId { get; init; } = groupId;
        [JsonPropertyName("album_id")] public required string AlbumId { get; init; } = albumId;
        [JsonPropertyName("media_id")] public required string MediaId { get; init; } = mediaId;
        [JsonPropertyName("batch_id")] public string? BatchId { get; init; } = batchId;
    }
}
