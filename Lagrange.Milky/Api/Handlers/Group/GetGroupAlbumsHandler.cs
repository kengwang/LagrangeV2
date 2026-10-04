using FastEndpoints;
using System.Text.Json.Serialization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.Group;

public sealed class GetGroupAlbumsHandler(BotContext lagrange) : Endpoint<GetGroupAlbumsHandler.Request, MilkyApiResponse<GetGroupAlbumsHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/get_group_album_list");
    }
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        var result = await lagrange.GetGroupAlbums(request.GroupId, request.AttachInfo ?? string.Empty, ct).WaitAsync(ct);
        return new(new Result(result));
    }

    public sealed class Request(long groupId, string? attachInfo = null)
    {
        [JsonPropertyName("group_id")] public long GroupId { get; init; } = groupId;
        [JsonPropertyName("attach_info")] public string? AttachInfo { get; init; } = attachInfo;
    }

    public sealed class Result(Lagrange.Core.Common.Response.BotGroupAlbumResult result)
    {
        [JsonPropertyName("albums")] public Album[] Albums { get; } = result.Albums.Select(x => new Album(x)).ToArray();
        [JsonPropertyName("has_more")] public bool HasMore { get; } = result.HasMore;
        [JsonPropertyName("attach_info")] public string AttachInfo { get; } = result.AttachInfo;
    }

    public sealed class Album(Lagrange.Core.Common.Response.BotGroupAlbum album)
    {
        [JsonPropertyName("album_id")] public string AlbumId { get; } = album.AlbumId;
        [JsonPropertyName("owner")] public string Owner { get; } = album.Owner;
        [JsonPropertyName("name")] public string Name { get; } = album.Name;
        [JsonPropertyName("description")] public string Description { get; } = album.Description;
        [JsonPropertyName("create_time")] public ulong CreateTime { get; } = album.CreateTime;
        [JsonPropertyName("modify_time")] public ulong ModifyTime { get; } = album.ModifyTime;
        [JsonPropertyName("last_upload_time")] public ulong LastUploadTime { get; } = album.LastUploadTime;
        [JsonPropertyName("upload_number")] public ulong UploadNumber { get; } = album.UploadNumber;
        [JsonPropertyName("cover_url")] public string? CoverUrl { get; } = album.CoverUrl;
    }
}
