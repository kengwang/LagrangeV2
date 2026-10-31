using Lagrange.Core.Common;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Web;
using Lagrange.Core.Services;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Internal.Services.Message;

[EventSubscribe<GetAlbumFeedDetailEventReq>(Protocols.All)]
[Service("QunAlbum.trpc.qzone.webapp_qun_feeds.FeedsReader.GetQunFeedDetail")]
internal sealed class GetAlbumFeedDetailService : BaseService<GetAlbumFeedDetailEventReq, GetAlbumFeedDetailEventResp>
{
    protected override ValueTask<ReadOnlyMemory<byte>> Build(GetAlbumFeedDetailEventReq input, BotContext context) => ValueTask.FromResult(ProtoHelper.Serialize(new AlbumFeedDetailRequest
    {
        Data = new AlbumFeedDetailData { GroupId = input.GroupUin.ToString(), AlbumId = input.AlbumId, BatchId = input.BatchId.ToString(global::System.Globalization.CultureInfo.InvariantCulture), Lloc = input.Lloc },
        TraceId = $"_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}", ExtMap = [new AlbumExtMapEntry { Key = "fc-appid", Value = "100" }]
    }));

    protected override ValueTask<GetAlbumFeedDetailEventResp> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        var response = ProtoHelper.Deserialize<AlbumFeedDetailResponse>(input.Span);
        if (response.Result != 0) throw new OperationException(response.Result, response.ErrorText);
        var cells = response.Data?.Feed?.Feed ?? throw new InvalidDataException("Group album feed detail is missing.");
        var header = ProtoHelper.Deserialize<AlbumFeedHeader>(cells.Header);
        if (string.IsNullOrWhiteSpace(header.FeedId) || cells.UserWrap.Length == 0 || cells.PhotoInfo.Length == 0) throw new InvalidDataException("Group album feed cells are incomplete.");
        if (string.IsNullOrWhiteSpace(ProtoHelper.Deserialize<AlbumCommentUserWrap>(cells.UserWrap).User.Uin)) throw new InvalidDataException("Group album feed owner is missing.");
        return ValueTask.FromResult(new GetAlbumFeedDetailEventResp(cells));
    }
}
