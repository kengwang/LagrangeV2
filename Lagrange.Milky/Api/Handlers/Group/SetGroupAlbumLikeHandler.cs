using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.Group;

public sealed class SetGroupAlbumLikeHandler(BotContext lagrange) : Endpoint<SetGroupAlbumLikeHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/set_group_album_like");
    }
    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
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
