using Lagrange.Core.Common;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Services;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Internal.Services.Message;

[Service("trpc.msg.msg_svc.MsgService.SsoReadedReport")]
[EventSubscribe<MarkMessageReadEventReq>(Protocols.All)]
[EventSubscribe<MarkAllMessagesReadEventReq>(Protocols.All)]
internal sealed class MarkMessageReadService : BaseService<MarkMessageReadEventReq, MarkMessageReadEventResp>
{
    protected override async ValueTask<ReadOnlyMemory<byte>> Build(MarkMessageReadEventReq input, BotContext context)
    {
        if (input.PeerUin <= 0) throw new InvalidTargetException(input.PeerUin);
        var request = new SsoReadedReportReq();
        if (input.Scene == "group") request.GroupList = [new GroupReadedReportItem { GroupUin = (ulong)input.PeerUin, LastReadSeq = input.LastReadSeq }];
        else if (input.Scene == "private")
        {
            var uid = context.CacheContext.ResolveCachedUid(input.PeerUin);
            if (uid is null)
            {
                await context.CacheContext.GetFriendList(true);
                uid = context.CacheContext.ResolveCachedUid(input.PeerUin);
            }
            request.C2CList = [new C2CReadedReportItem { Uid = uid ?? throw new InvalidTargetException(input.PeerUin), LastReadSeq = input.LastReadSeq }];
        }
        else throw new ArgumentException("Message scene must be group or private.", nameof(input));
        return ProtoHelper.Serialize(request);
    }

    protected override ValueTask<MarkMessageReadEventResp> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        if (input.IsEmpty) throw new OperationException(-1, "Read report response is empty.");
        var response = ProtoHelper.Deserialize<SsoReadedReportResp>(input.Span);
        if (response.ResultCode != 0) throw new OperationException((int)response.ResultCode, response.ErrorMessage);
        var error = response.GroupList?.FirstOrDefault(x => x.ResultCode != 0)?.ErrorMessage ?? response.C2CList?.FirstOrDefault(x => x.ResultCode != 0)?.ErrorMessage;
        if (error is not null) throw new OperationException(-1, error);
        return ValueTask.FromResult(new MarkMessageReadEventResp());
    }
}

[Service("trpc.msg.msg_svc.MsgService.SsoReadedReport")]
internal sealed class MarkAllMessagesReadService : BaseService<MarkAllMessagesReadEventReq, MarkAllMessagesReadEventResp>
{
    protected override async ValueTask<ReadOnlyMemory<byte>> Build(MarkAllMessagesReadEventReq input, BotContext context)
    {
        var request = new SsoReadedReportReq
        {
            GroupList = [.. input.GroupUins.Where(x => x > 0).Select(x => new GroupReadedReportItem { GroupUin = (ulong)x })],
            C2CList = []
        };
        foreach (var uin in input.PrivateUins.Where(x => x > 0))
        {
            var uid = context.CacheContext.ResolveCachedUid(uin);
            if (uid is null) { await context.CacheContext.GetFriendList(true); uid = context.CacheContext.ResolveCachedUid(uin); }
            if (uid is null) throw new InvalidTargetException(uin);
            request.C2CList.Add(new C2CReadedReportItem { Uid = uid, LastReadTime = (ulong)DateTimeOffset.UtcNow.ToUnixTimeSeconds() });
        }
        return ProtoHelper.Serialize(request);
    }

    protected override ValueTask<MarkAllMessagesReadEventResp> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        if (input.IsEmpty) throw new OperationException(-1, "Read report response is empty.");
        var response = ProtoHelper.Deserialize<SsoReadedReportResp>(input.Span);
        if (response.ResultCode != 0) throw new OperationException((int)response.ResultCode, response.ErrorMessage);
        var groupErrors = response.GroupList?.FirstOrDefault(x => x.ResultCode != 0);
        var privateErrors = response.C2CList?.FirstOrDefault(x => x.ResultCode != 0);
        if (groupErrors is not null) throw new OperationException((int)groupErrors.ResultCode, groupErrors.ErrorMessage);
        if (privateErrors is not null) throw new OperationException((int)privateErrors.ResultCode, privateErrors.ErrorMessage);
        return ValueTask.FromResult(new MarkAllMessagesReadEventResp(
            [.. (response.GroupList ?? []).Select(x => ((long)x.GroupUin, x.LatestSeq))],
            [.. (response.C2CList ?? []).Select(x => (x.TargetUin != 0 ? (long)x.TargetUin : ResolvePrivateUin(context, x), x.LatestSeq))]));
    }

    private static long ResolvePrivateUin(BotContext context, C2CReadedReportResponseItem item) =>
        !string.IsNullOrWhiteSpace(item.Uid) ? context.CacheContext.ResolveUin(item.Uid) : 0;
}
