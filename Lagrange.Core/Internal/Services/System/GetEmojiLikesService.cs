using Lagrange.Core.Common;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<GetEmojiLikesEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x9083_1")]
internal sealed class GetEmojiLikesService : OidbService<GetEmojiLikesEventReq, GetEmojiLikesEventResp, D9083ReqBody, D9083RespBody>
{
    protected override uint Command => 0x9083;
    protected override uint Service => 1;

    protected override Task<D9083ReqBody> ProcessRequest(GetEmojiLikesEventReq request, BotContext context) =>
        Task.FromResult(new D9083ReqBody
        {
            GroupId = checked((ulong)request.GroupUin),
            Sequence = request.Sequence,
            EmojiType = request.Code.Length <= 3 ? 1u : 2u,
            EmojiId = request.Code,
            Cookie = request.Cookie,
            Field7 = 1,
            Count = request.Count,
        });

    protected override Task<GetEmojiLikesEventResp> ProcessResponse(D9083RespBody response, BotContext context) =>
        Task.FromResult(new GetEmojiLikesEventResp(new BotEmojiLikesResult
        {
            Users = [.. (response.Users ?? []).Select(user => new BotEmojiLikeUser
            {
                Uin = checked((long)user.Uin),
                Nickname = user.Nickname ?? string.Empty,
                HeadUrl = user.HeadUrl ?? string.Empty,
            })],
            Cookie = response.Cookie ?? string.Empty,
            IsLast = response.IsLast,
        }));
}
