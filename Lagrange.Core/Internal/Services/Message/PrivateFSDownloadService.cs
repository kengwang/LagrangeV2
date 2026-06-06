using System.Text;
using Lagrange.Core.Common;
using Lagrange.Core.Internal.Events;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Packets.Service;

namespace Lagrange.Core.Internal.Services.Message;

[EventSubscribe<PrivateFSDownloadEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0xe37_1200")]
internal class PrivateFSDownloadService : OidbService<PrivateFSDownloadEventReq, PrivateFSDownloadEventResp, PrivateFSDownloadRequest, PrivateFSDownloadResponse>
{
    private protected override uint Command => 0xe37;

    private protected override uint Service => 1200;

    private protected override Task<PrivateFSDownloadRequest> ProcessRequest(PrivateFSDownloadEventReq request, BotContext context)
    {
        return Task.FromResult(new PrivateFSDownloadRequest
        {
            Body = new PrivateFSDownloadRequestBody
            {
                ReceiverUid = request.ReceiverUid,
                FileUuid = request.FileId,
                FileHash = request.FileHash,
                T2 = 0
            }
        });
    }

    private protected override Task<PrivateFSDownloadEventResp> ProcessResponse(PrivateFSDownloadResponse response, BotContext context)
    {
        var result = response.Body.Result;
        var url = new StringBuilder()
            .Append("http://")
            .Append(result.Server).Append(':').Append(result.Port)
            .Append(result.Url).Append("&isthumb=0")
            .ToString();

        return Task.FromResult(new PrivateFSDownloadEventResp(url));
    }
}
