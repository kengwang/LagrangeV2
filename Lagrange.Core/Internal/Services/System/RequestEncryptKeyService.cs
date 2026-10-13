using Lagrange.Core.Common;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<RequestEncryptKeyEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0xcde_1")]
internal sealed class RequestEncryptKeyService
    : OidbService<RequestEncryptKeyEventReq, RequestEncryptKeyEventResp, RequestEncryptKeyBody, RequestEncryptKeyResponse>
{
    protected override uint Command => 0xcde;
    protected override uint Service => 1;

    protected override Task<RequestEncryptKeyBody> ProcessRequest(RequestEncryptKeyEventReq request, BotContext context)
        => Task.FromResult(new RequestEncryptKeyBody());

    protected override Task<RequestEncryptKeyEventResp> ProcessResponse(RequestEncryptKeyResponse response, BotContext context)
    {
        if (string.IsNullOrEmpty(response.Info?.Key) || string.IsNullOrEmpty(response.Info.KeyMeta))
            throw new OperationException(-500, "Database key or key metadata was not returned.");
        return Task.FromResult(new RequestEncryptKeyEventResp(new BotDatabaseKeyResult(response.Info.Key, response.Info.KeyMeta)));
    }
}
