using System.Globalization;
using Lagrange.Core.Common;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<GetSystemFaceEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x9155_1")]
internal sealed class GetSystemFaceService() : OidbService<GetSystemFaceEventReq, GetSystemFaceEventResp, D9155Req, D9155Resp>(0x9155, 1)
{
    protected override Task<D9155Req> ProcessRequest(GetSystemFaceEventReq request, BotContext context) => Task.FromResult(new D9155Req { Sid = request.FaceId.ToString(CultureInfo.InvariantCulture) });
    protected override Task<GetSystemFaceEventResp> ProcessResponse(D9155Resp response, BotContext context)
    {
        if (response.Emoji is null) return Task.FromResult(new GetSystemFaceEventResp(null));
        var faces = GetSystemFacesService.Convert([response.Emoji]);
        if (faces.Count != 1) throw new InvalidDataException("System face lookup response has no identity.");
        return Task.FromResult(new GetSystemFaceEventResp(faces[0]));
    }
}
