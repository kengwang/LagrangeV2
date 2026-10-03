using Lagrange.Core.Common;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<GetProfileLikeEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x7ed_13")]
internal sealed class GetProfileLikeService : OidbService<GetProfileLikeEventReq, GetProfileLikeEventResp, D7EDReqBody, D7EDRespBody>
{
    protected override uint Command => 0x7ed;
    protected override uint Service => 13;

    protected override async Task<D7EDReqBody> ProcessRequest(GetProfileLikeEventReq request, BotContext context)
    {
        var uid = request.UserUin is null
            ? context.CacheContext.ResolveCachedUid(context.BotUin)
            : context.CacheContext.ResolveCachedUid(request.UserUin.Value);
        if (string.IsNullOrWhiteSpace(uid))
        {
            await context.CacheContext.GetFriendList(true);
            uid = request.UserUin is null
                ? context.CacheContext.ResolveCachedUid(context.BotUin)
                : context.CacheContext.ResolveCachedUid(request.UserUin.Value);
        }
        if (string.IsNullOrWhiteSpace(uid)) throw new InvalidTargetException(request.UserUin ?? context.BotUin);
        return new D7EDReqBody { TargetUids = [uid], Basic = 1, Vote = 1, UserProfile = 1, Start = checked((uint)request.Start), Limit = checked((uint)request.Limit) };
    }

    protected override Task<GetProfileLikeEventResp> ProcessResponse(D7EDRespBody response, BotContext context)
    {
        var info = response.UserLikeInfos.FirstOrDefault() ?? throw new OperationException(-1, "0x7ed returned an empty like response.");
        var users = (info.VoteInfo?.Users ?? []).Select(user => new BotProfileLikeUser { Uid = user.Uid, Uin = context.CacheContext.ResolveUin(user.Uid), Nickname = user.Nickname, LatestTime = user.LatestTime, Count = checked((int)user.Count), IsFriend = user.IsFriend }).ToArray();
        return Task.FromResult(new GetProfileLikeEventResp(new BotProfileLikeResult { Uid = info.Uid, Time = info.Time, TotalCount = checked((int)(info.VoteInfo?.TotalCount ?? 0)), NewCount = checked((int)(info.VoteInfo?.NewCount ?? 0)), Users = users }));
    }
}
