using Lagrange.Core.Common;
using Lagrange.Core.Events;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<GetCustomFaceListEventReq>(Protocols.All)]
[EventSubscribe<DeleteCustomFaceEventReq>(Protocols.All)]
[Service("Faceroam.OpReq")]
internal sealed class GetCustomFaceListService : BaseService<ProtocolEvent, ProtocolEvent>
{
    private readonly Queue<Type> _pending = new();

    protected override ValueTask<ReadOnlyMemory<byte>> Build(ProtocolEvent input, BotContext context)
    {
        FaceroamRequest request = input switch
        {
            GetCustomFaceListEventReq => new FaceroamRequest
            {
                Inner = new FaceroamInner { Field1 = 1, OsVersion = "10.0.26200", QqVersion = "9.9.28-46928" }, Uin = checked((ulong)context.BotUin), Field3 = 1, Field6 = 1
            },
            DeleteCustomFaceEventReq delete => new FaceroamRequest
            {
                Inner = new FaceroamInner { Field1 = 1, OsVersion = "10.0.26200" }, Uin = checked((ulong)context.BotUin), Field3 = 2, Body = new FaceroamBody { EmojiId = delete.FaceId }
            },
            _ => throw new InvalidOperationException($"Unsupported Faceroam event {input.GetType().Name}")
        };
        lock (_pending) _pending.Enqueue(input is DeleteCustomFaceEventReq ? typeof(DeleteCustomFaceEventResp) : typeof(GetCustomFaceListEventResp));
        return ValueTask.FromResult<ReadOnlyMemory<byte>>(ProtoHelper.Serialize(request));
    }

    protected override ValueTask<ProtocolEvent> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        var response = ProtoHelper.Deserialize<FaceroamResponse>(input.Span);
        if (response.RetCode != 0) throw new OperationException((int)response.RetCode, response.Message);
        Type type;
        lock (_pending) type = _pending.Dequeue();
        if (type == typeof(DeleteCustomFaceEventResp)) return ValueTask.FromResult<ProtocolEvent>(DeleteCustomFaceEventResp.Default);
        return ValueTask.FromResult<ProtocolEvent>(new GetCustomFaceListEventResp(new BotCustomFaceListResult
        {
            FaceIds = response.Item?.FaceIds ?? [],
            TotalCount = response.Item?.TotalCount ?? 0,
        }));
    }
}
