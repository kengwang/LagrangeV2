using System.Globalization;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Web;
using Lagrange.Core.Services;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Internal.Services.Message;

[EventSubscribe<GetGroupAlbumsEventReq>(Protocols.All)]
[Service("QunAlbum.trpc.qzone.webapp_qun_media.QunMedia.GetAlbumList")]
internal sealed class GetGroupAlbumsService : BaseService<GetGroupAlbumsEventReq, GetGroupAlbumsEventResp>
{
    protected override ValueTask<ReadOnlyMemory<byte>> Build(GetGroupAlbumsEventReq input, BotContext context)
    {
        if (input.GroupUin <= 0) throw new ArgumentOutOfRangeException(nameof(input.GroupUin));
        ArgumentNullException.ThrowIfNull(input.AttachInfo);
        if (input.AttachInfo.Length > 4096) throw new ArgumentOutOfRangeException(nameof(input.AttachInfo));
        return ValueTask.FromResult(ProtoHelper.Serialize(new GetAlbumListRequest
        {
            Data = new() { GroupId = input.GroupUin.ToString(CultureInfo.InvariantCulture), AttachInfo = input.AttachInfo },
            TraceId = $"_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{Random.Shared.Next(100000)}",
            ExtMap = [new() { Key = "fc-appid", Value = "100" }]
        }));
    }

    protected override ValueTask<GetGroupAlbumsEventResp> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        if (input.IsEmpty) throw new OperationException(-1, "Group album response is empty.");
        var response = ProtoHelper.Deserialize<GetAlbumListResponse>(input.Span);
        if (response.Result != 0) throw new OperationException(response.Result, response.ErrorText);
        var data = response.Data ?? throw new OperationException(-1, "Group album response is missing its data.");
        return ValueTask.FromResult(new GetGroupAlbumsEventResp(new BotGroupAlbumResult
        {
            Albums = data.Albums.Select(album => new BotGroupAlbum
            {
                AlbumId = album.AlbumId, Owner = album.Owner, Name = album.Name, Description = album.Description,
                CreateTime = album.CreateTime, ModifyTime = album.ModifyTime, LastUploadTime = album.LastUploadTime,
                UploadNumber = album.UploadNumber,
                CoverUrl = album.Cover?.Image?.DefaultUrl?.Url ?? album.Cover?.Image?.PhotoUrls.FirstOrDefault()?.Url?.Url
                    ?? album.Cover?.Video?.Cover?.DefaultUrl?.Url
            }).ToArray(),
            AttachInfo = data.AttachInfo,
            HasMore = data.HasMore
        }));
    }
}
