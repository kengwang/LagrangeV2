using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.Group;

[ApiHandler("get_group_album_media")]
public sealed class GetGroupAlbumMediaHandler(BotContext lagrange) : IApiHandler<GetGroupAlbumMediaHandler.Request, GetGroupAlbumMediaHandler.Result>
{
    public async ValueTask<MilkyApiResponse<Result>> HandleAsync(Request request, CancellationToken ct)
    {
        var result = await lagrange.GetGroupAlbumMedia(request.GroupId, request.AlbumId, request.AttachInfo ?? string.Empty, ct).WaitAsync(ct);
        return new(new Result(result));
    }
    public sealed class Request(long groupId, string albumId, string? attachInfo = null)
    {
        [JsonPropertyName("group_id")] public long GroupId { get; init; } = groupId;
        [JsonPropertyName("album_id")] public required string AlbumId { get; init; } = albumId;
        [JsonPropertyName("attach_info")] public string? AttachInfo { get; init; } = attachInfo;
    }
    public sealed class Result(Lagrange.Core.Common.Response.BotGroupAlbumMediaResult result)
    {
        [JsonPropertyName("media")] public Media[] Media { get; } = result.Media.Select(x => new Media(x)).ToArray();
        [JsonPropertyName("previous_cursor")] public string PreviousCursor { get; } = result.PreviousCursor;
        [JsonPropertyName("next_cursor")] public string NextCursor { get; } = result.NextCursor;
    }
    public sealed class Media(Lagrange.Core.Common.Response.BotGroupAlbumMedia media)
    {
        [JsonPropertyName("type")] public string Type { get; } = media.Type;
        [JsonPropertyName("id")] public string? Id { get; } = media.Id;
        [JsonPropertyName("url")] public string? Url { get; } = media.Url;
        [JsonPropertyName("cover_url")] public string? CoverUrl { get; } = media.CoverUrl;
        [JsonPropertyName("width")] public uint Width { get; } = media.Width;
        [JsonPropertyName("height")] public uint Height { get; } = media.Height;
        [JsonPropertyName("upload_time")] public ulong UploadTime { get; } = media.UploadTime;
    }
}
