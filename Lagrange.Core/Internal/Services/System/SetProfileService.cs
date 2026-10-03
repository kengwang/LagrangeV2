using Lagrange.Core.Common;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<SetProfileEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x112a_2")]
internal sealed class SetProfileService : OidbService<SetProfileEventReq, SetProfileEventResp, D112AReqBody, D112ARespBody>
{
    protected override uint Command => 0x112a;
    protected override uint Service => 2;

    protected override Task<D112AReqBody> ProcessRequest(SetProfileEventReq request, BotContext context)
    {
        var strings = new List<D112AProfileString>();
        if (request.Nickname is not null) strings.Add(new D112AProfileString { FieldId = 20002, Value = request.Nickname });
        if (request.PersonalNote is not null) strings.Add(new D112AProfileString { FieldId = 102, Value = request.PersonalNote });
        List<D112AProfileInt>? ints = request.Sex is int sex
            ? [new D112AProfileInt { FieldId = 20009, Value = checked((ulong)sex) }]
            : null;
        return Task.FromResult(new D112AReqBody { Uin = checked((ulong)context.BotUin), StringProfiles = strings, IntProfiles = ints });
    }

    protected override Task<SetProfileEventResp> ProcessResponse(D112ARespBody response, BotContext context) =>
        Task.FromResult(SetProfileEventResp.Default);
}
