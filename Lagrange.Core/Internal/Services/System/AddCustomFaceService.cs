using Lagrange.Core.Common;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<AddCustomFaceEventReq>(Protocols.All)]
[Service("ImgStore.BDHExpressionRoam")]
internal sealed class AddCustomFaceService : BaseService<AddCustomFaceEventReq, AddCustomFaceEventResp>
{
    protected override ValueTask<ReadOnlyMemory<byte>> Build(AddCustomFaceEventReq input, BotContext context) => ValueTask.FromResult<ReadOnlyMemory<byte>>(ProtoHelper.Serialize(new BdhExpressionRoamRequest
    {
        Field1 = 3, Field2 = 1, Inner = new BdhExpressionRoamInner { Field1 = 0, Uin = checked((ulong)context.Keystore.Uin), Field3 = 0, Md5 = input.Md5, FileSize = input.FileSize, Field7 = 2, Field8 = 0, Field9 = 1, Version = "1.0.0", Field16 = 1 },
        Field7 = 9, Tail = new BdhExpressionRoamTail { Inner = new BdhExpressionRoamTailInner { Field1 = 0, Field2 = 0, Field3 = "0" }, Field2 = 1 }
    }));

    protected override ValueTask<AddCustomFaceEventResp> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        var response = ProtoHelper.Deserialize<BdhExpressionRoamResponse>(input.Span);
        var token = response.Inner?.Token ?? [];
        if (token.Length == 0) throw new OperationException(-1, "BDHExpressionRoam response did not contain an upload token.");
        return ValueTask.FromResult(new AddCustomFaceEventResp(token));
    }
}
