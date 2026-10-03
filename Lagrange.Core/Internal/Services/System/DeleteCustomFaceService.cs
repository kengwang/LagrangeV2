using Lagrange.Core.Common;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Internal.Services.System;

internal sealed class DeleteCustomFaceService : BaseService<DeleteCustomFaceEventReq, DeleteCustomFaceEventResp>
{
    protected override ValueTask<ReadOnlyMemory<byte>> Build(DeleteCustomFaceEventReq input, BotContext context) =>
        ValueTask.FromResult<ReadOnlyMemory<byte>>(ProtoHelper.Serialize(new FaceroamRequest
        {
            Inner = new FaceroamInner { Field1 = 1, OsVersion = "10.0.26200" },
            Uin = checked((ulong)context.BotUin),
            Field3 = 2,
            Body = new FaceroamBody { EmojiId = input.FaceId },
        }));

    protected override ValueTask<DeleteCustomFaceEventResp> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        var response = ProtoHelper.Deserialize<FaceroamResponse>(input.Span);
        if (response.RetCode != 0) throw new OperationException((int)response.RetCode, response.Message);
        return ValueTask.FromResult(DeleteCustomFaceEventResp.Default);
    }
}
