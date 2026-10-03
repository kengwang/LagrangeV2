using Lagrange.Core.Common;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<GetGroupReactionSummaryEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x9084_1")]
internal sealed class GetGroupReactionSummaryService : OidbService<GetGroupReactionSummaryEventReq, GetGroupReactionSummaryEventResp, D9084ReqBody, D9084RespBody>
{
    protected override uint Command => 0x9084;
    protected override uint Service => 1;

    protected override Task<D9084ReqBody> ProcessRequest(GetGroupReactionSummaryEventReq request, BotContext context) =>
        Task.FromResult(new D9084ReqBody
        {
            GroupId = checked((ulong)request.GroupUin),
            Sequence = request.Sequence,
            EmojiId = string.Empty,
            EmojiType = 0,
            Cookie = string.Empty,
            Count = 0,
            Field12 = 1,
        });

    protected override Task<GetGroupReactionSummaryEventResp> ProcessResponse(D9084RespBody response, BotContext context) =>
        Task.FromResult(new GetGroupReactionSummaryEventResp(new BotGroupReactionSummaryResult
        {
            Entries = [.. (response.Entries ?? []).Where(entry => entry.Count > 0).Select(entry => new BotGroupReactionSummaryEntry
            {
                EmojiId = entry.EmojiId ?? string.Empty,
                EmojiType = entry.EmojiType,
                Count = entry.Count,
                LastReactionTime = entry.LastReactionTime,
            })],
        }));
}
