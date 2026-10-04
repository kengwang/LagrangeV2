using Lagrange.Core.Common;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Web;
using Lagrange.Core.Services;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Internal.Services.Message;

[EventSubscribe<GetGroupAlbumMediaEventReq>(Protocols.All)]
[Service("QunAlbum.trpc.qzone.webapp_qun_media.QunMedia.GetMediaList")]
internal sealed class GetGroupAlbumMediaService : BaseService<GetGroupAlbumMediaEventReq, GetGroupAlbumMediaEventResp>
{
    protected override ValueTask<ReadOnlyMemory<byte>> Build(GetGroupAlbumMediaEventReq input, BotContext context)
    {
        if (input.GroupUin <= 0 || string.IsNullOrWhiteSpace(input.AlbumId)) throw new ArgumentException("Group and album are required.");
        return ValueTask.FromResult(ProtoHelper.Serialize(new GetMediaListRequest { Field1 = 0, ReqInfo = new GetMediaListReqInfo { GroupId = input.GroupUin.ToString(), AlbumId = input.AlbumId, PageInfo = input.Cursor }, TraceId = $"_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}", ExtMap = [new AlbumExtMapEntry { Key = "fc-appid", Value = "100" }] }));
    }
    protected override ValueTask<GetGroupAlbumMediaEventResp> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        if (input.IsEmpty) throw new OperationException(-1, "Group album media response is empty.");
        var response = ProtoHelper.Deserialize<GetMediaListResponse>(input.Span);
        if (response.Field1 != 0) throw new OperationException(response.Field1, "Group album media request failed.");
        // QQ media type values differ between album/feed codec variants. The actual
        // video payload is authoritative when determining the media type.
        var media = (response.Data?.MediaList ?? []).Select(item => item.Video is { } video
            ? new BotGroupAlbumMedia
            {
                Type = "video", Id = video.Id,
                Url = !string.IsNullOrEmpty(video.Url) ? video.Url : video.VideoUrl.FirstOrDefault(url => !string.IsNullOrEmpty(url.Url?.Url))?.Url?.Url,
                CoverUrl = ImageUrl(video.Cover)?.Url, Width = video.Width, Height = video.Height, UploadTime = item.UploadTime
            }
            : new BotGroupAlbumMedia
            {
                Type = "image", Id = item.Image?.Lloc, Url = ImageUrl(item.Image)?.Url,
                Width = ImageUrl(item.Image)?.Width ?? 0, Height = ImageUrl(item.Image)?.Height ?? 0, UploadTime = item.UploadTime
            }).ToList();
        return ValueTask.FromResult(new GetGroupAlbumMediaEventResp(new BotGroupAlbumMediaResult { Media = media, PreviousCursor = response.Data?.PrevAttachInfo ?? string.Empty, NextCursor = response.Data?.NextAttachInfo ?? string.Empty }));
    }

    private static AlbumUrlInfo? ImageUrl(AlbumImageInfo? image) => !string.IsNullOrEmpty(image?.DefaultUrl?.Url)
        ? image?.DefaultUrl : image?.PhotoUrls.FirstOrDefault(url => !string.IsNullOrEmpty(url.Url?.Url))?.Url;
}

[EventSubscribe<SetGroupAlbumLikeEventReq>(Protocols.All)]
[Service("QunAlbum.trpc.qzone.webapp_qun_operation.FeedsWriter.DoQunLike")]
internal sealed class SetGroupAlbumLikeService : BaseService<SetGroupAlbumLikeEventReq, SetGroupAlbumLikeEventResp>
{
    protected override ValueTask<ReadOnlyMemory<byte>> Build(SetGroupAlbumLikeEventReq input, BotContext context)
    {
        var id = input.MediaId is null ? $"421_1_0_{input.GroupUin}|{input.AlbumId}|{input.BatchId}" : $"421_1_0_{input.GroupUin}|{input.AlbumId}|{input.BatchId}^||^421_1_0_{input.GroupUin}|{input.AlbumId}|{input.MediaId}^||^0";
        var uin = context.BotUin.ToString();
        var request = new DoQunLikeRequest { Field1 = 5495, Field2 = "h5_test", Field3 = "h5_test", Body = new AlbumLikeBody { Type = input.IsLike ? 2u : 1u, Like = new AlbumLikeInfo { Id = id, Status = input.IsLike ? 0u : 1u }, Publish = new AlbumLikePublish { CellCommon = new AlbumLikeCellCommon { Time = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), FeedId = $"422_0_{input.BatchId}" }, CellUserInfo = new AlbumLikeUserInfo { User = new AlbumLikeUser { Uin = uin } }, CellMedia = new AlbumLikeMedia { AlbumId = input.AlbumId, BatchId = ulong.TryParse(input.BatchId, out var parsed) ? parsed : 0 }, CellQunInfo = new AlbumLikeQun { QunId = input.GroupUin.ToString() } }, ClientKey = $"{uin}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}" }, ExtMap = [new AlbumExtMapEntry { Key = "fc-appid", Value = "100" }] };
        return ValueTask.FromResult(ProtoHelper.Serialize(request));
    }
    protected override ValueTask<SetGroupAlbumLikeEventResp> Parse(ReadOnlyMemory<byte> input, BotContext context) { var response = ProtoHelper.Deserialize<DoQunLikeResponse>(input.Span); if (response.Field1 != 5495) throw new OperationException(response.Field1, "Group album like request failed."); return ValueTask.FromResult(SetGroupAlbumLikeEventResp.Instance); }
}

[EventSubscribe<DeleteGroupAlbumMediaEventReq>(Protocols.All)]
[Service("QunAlbum.trpc.qzone.webapp_qun_media.QunMedia.DeleteMedias")]
internal sealed class DeleteGroupAlbumMediaService : BaseService<DeleteGroupAlbumMediaEventReq, DeleteGroupAlbumMediaEventResp>
{
    protected override ValueTask<ReadOnlyMemory<byte>> Build(DeleteGroupAlbumMediaEventReq input, BotContext context) => ValueTask.FromResult(ProtoHelper.Serialize(new DeleteMediasRequest { Field1 = 8694, Field2 = "h5_test", Field3 = "h5_test", Body = new DeleteMediasReqBody { GroupId = input.GroupUin.ToString(), AlbumId = input.AlbumId, MediaIds = [input.MediaId], BatchIds = string.IsNullOrWhiteSpace(input.BatchId) ? [] : [input.BatchId] }, TraceId = $"_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}", ExtMap = [new AlbumExtMapEntry { Key = "fc-appid", Value = "100" }] }));
    protected override ValueTask<DeleteGroupAlbumMediaEventResp> Parse(ReadOnlyMemory<byte> input, BotContext context) { var response = ProtoHelper.Deserialize<DeleteMediasResponse>(input.Span); if (response.Field1 != 8694 || response.Field2 != 0) throw new OperationException(response.Field2 == 0 ? response.Field1 : response.Field2, response.Field3 ?? "Group album delete failed."); return ValueTask.FromResult(DeleteGroupAlbumMediaEventResp.Instance); }
}

[EventSubscribe<CommentGroupAlbumMediaEventReq>(Protocols.All)]
[Service("QunAlbum.trpc.qzone.webapp_qun_operation.FeedsWriter.DoQunComment")]
internal sealed class CommentGroupAlbumMediaService : BaseService<CommentGroupAlbumMediaEventReq, CommentGroupAlbumMediaEventResp>
{
    protected override ValueTask<ReadOnlyMemory<byte>> Build(CommentGroupAlbumMediaEventReq input, BotContext context)
    {
        var uin = context.BotUin.ToString();
        var request = new DoQunCommentRequest { Field1 = 8527, Body = new DoQunCommentBody { GroupId = input.GroupUin.ToString(), Field3 = 2, ReqBody = new AlbumCommentReqBody { Header = new AlbumCommentHeader(), UserWrap = new AlbumCommentUserWrap { User = new AlbumCommentUser { Uin = uin } }, PhotoInfo = new AlbumCommentPhotoInfo { AlbumId = input.AlbumId, Wrap = new AlbumCommentPhotoWrap { Meta = new AlbumCommentPhotoMeta { Lloc = input.MediaId } } } }, Content = new AlbumCommentContent { User = new AlbumCommentUser { Uin = uin }, Meta = new AlbumCommentContentMeta { Content = input.Content }, ClientKey = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString() } }, TraceId = $"_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}", ExtMap = [new AlbumExtMapEntry { Key = "fc-appid", Value = "100" }] };
        return ValueTask.FromResult(ProtoHelper.Serialize(request));
    }
    protected override ValueTask<CommentGroupAlbumMediaEventResp> Parse(ReadOnlyMemory<byte> input, BotContext context) { var response = ProtoHelper.Deserialize<DoQunCommentResponse>(input.Span); if (response.Field1 != 0 && response.Field1 != 8527 && response.Comment is null) throw new OperationException(response.Field1, "Group album comment failed."); return ValueTask.FromResult(new CommentGroupAlbumMediaEventResp(response.Comment?.Data?.Id ?? string.Empty)); }
}
