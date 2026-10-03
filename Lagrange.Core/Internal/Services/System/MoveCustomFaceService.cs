using Lagrange.Core.Common;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<MoveCustomFaceEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x902f_1")]
internal sealed class MoveCustomFaceService : BaseService<MoveCustomFaceEventReq, MoveCustomFaceEventResp>
{
    protected override async ValueTask<ReadOnlyMemory<byte>> Build(MoveCustomFaceEventReq input, BotContext context)
    {
        var order = new D902FBody
        {
            Env = new D902FEnv { Field1 = 1024, OsVersion = "10.0.26200", BuildVersion = "9.9.26" },
            EmojiId = input.FaceId,
            Position = input.Position,
        };
        var first = ProtoHelper.Serialize(new Oidb { Command = 0x902f, Service = 1, Reserved = 1, Body = ProtoHelper.Serialize(order) });
        return first;
    }

    protected override ValueTask<MoveCustomFaceEventResp> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        var oidb = ProtoHelper.Deserialize<Oidb>(input.Span);
        if (oidb.Result != 0) throw new OperationException((int)oidb.Result, oidb.Message);
        return ValueTask.FromResult(MoveCustomFaceEventResp.Default);
    }
}
