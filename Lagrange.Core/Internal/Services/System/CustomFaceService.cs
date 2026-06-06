using Lagrange.Core.Common;
using Lagrange.Core.Internal.Events;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<FetchCustomFaceEventReq>(Protocols.All)]
[Service("Faceroam.OpReq")]
internal class FetchCustomFaceService : BaseService<FetchCustomFaceEventReq, FetchCustomFaceEventResp>
{
    protected override ValueTask<ReadOnlyMemory<byte>> Build(FetchCustomFaceEventReq input, BotContext context)
    {
        var packet = new FaceRoamRequest
        {
            Comm = new FaceRoamPlatInfo
            {
                ImPlat = 1,
                OsVersion = context.AppInfo.Kernel,
                QVersion = context.AppInfo.CurrentVersion
            },
            SelfUin = context.Keystore.Uin,
            SubCmd = 1,
            Field6 = 1
        };

        return ValueTask.FromResult(ProtoHelper.Serialize(packet));
    }

    protected override ValueTask<FetchCustomFaceEventResp> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        var payload = ProtoHelper.Deserialize<FaceRoamResponse>(input.Span);
        var urls = payload.UserInfo.FileName.Select(name => $"https://p.qpic.cn/{payload.UserInfo.Bid}/{context.Keystore.Uin}/{name}/0").ToList();
        return ValueTask.FromResult(new FetchCustomFaceEventResp(urls));
    }
}
