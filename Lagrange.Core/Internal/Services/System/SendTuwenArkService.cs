using Lagrange.Core.Common;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<SendTuwenArkEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0xdc2_34")]
internal sealed class SendTuwenArkService : OidbService<SendTuwenArkEventReq, SendTuwenArkEventResp, DDC2Req, DEmptyResp>
{
    protected override uint Command => 0xdc2;
    protected override uint Service => 34;
    protected override Task<DDC2Req> ProcessRequest(SendTuwenArkEventReq request, BotContext context)
    {
        if (request.TargetId <= 0) throw new ArgumentOutOfRangeException(nameof(request.TargetId));
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.JumpUrl)) throw new ArgumentException("Title and jump URL are required.");
        return Task.FromResult(new DDC2Req
        {
            AppInfo = new DDC2AppInfo { AppId = 100446242, Field2 = 1, Field3 = 0, Field5 = new DDC2Field5 { Field1 = 1 }, TargetId = checked((uint)request.TargetId), Content = new DDC2Content { Flag = 1, Title = request.Title, Description = request.Description, Summary = request.Summary, JumpUrl = request.JumpUrl, PreviewUrl = request.PreviewUrl } },
            Meta = new DDC2Meta { PeerType = request.Group ? 1u : 0u, TargetId = checked((uint)request.TargetId) }
        });
    }
    protected override Task<SendTuwenArkEventResp> ProcessResponse(DEmptyResp response, BotContext context) => Task.FromResult(SendTuwenArkEventResp.Instance);
}
