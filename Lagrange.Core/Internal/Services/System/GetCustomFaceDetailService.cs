using Lagrange.Core.Common;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<GetCustomFaceDetailEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x902e_1")]
internal sealed class GetCustomFaceDetailService : OidbService<GetCustomFaceDetailEventReq, GetCustomFaceDetailEventResp, D902EBody, D902EResponse>
{
    protected override uint Command => 0x902e;
    protected override uint Service => 1;
    protected override uint Reserved => 1;

    protected override Task<D902EBody> ProcessRequest(GetCustomFaceDetailEventReq request, BotContext context) =>
        Task.FromResult(new D902EBody
        {
            Field1 = 1,
            OsVersion = "10.0.26200",
            OpType = 2,
            Emojis = [.. request.Entries.Select(entry => new D902EEmoji { EmojiId = entry.FaceId, Md5 = entry.Md5 })],
        });

    protected override Task<GetCustomFaceDetailEventResp> ProcessResponse(D902EResponse response, BotContext context)
    {
        if (response.RetCode != 0) throw new OperationException((int)response.RetCode, response.ErrorMessage);
        var entries = (response.Entries ?? []).Select((entry, index) =>
        {
            var id = entry.Emoji?.EmojiId;
            if (string.IsNullOrWhiteSpace(id)) throw new OperationException(-1, $"Custom face detail entry {index} has no face id.");
            return new BotCustomFaceDetail { FaceId = id, Description = entry.Description ?? entry.LegacyDescription ?? string.Empty };
        }).ToList();
        return Task.FromResult(new GetCustomFaceDetailEventResp(new BotCustomFaceDetailResult { Entries = entries }));
    }
}
