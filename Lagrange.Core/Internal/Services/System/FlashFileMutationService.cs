using Lagrange.Core.Common;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<DeleteFlashFileEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x9407_1")]
internal sealed class DeleteFlashFileService : OidbService<DeleteFlashFileEventReq, DeleteFlashFileEventResp, D9407Req, DEmptyResp>
{
    protected override uint Command => 0x9407;
    protected override uint Service => 1;
    protected override Task<D9407Req> ProcessRequest(DeleteFlashFileEventReq request, BotContext context) => Task.FromResult(new D9407Req { FilesetUuid = request.FilesetUuid, Field3 = 7 });
    protected override Task<DeleteFlashFileEventResp> ProcessResponse(DEmptyResp response, BotContext context) => Task.FromResult(new DeleteFlashFileEventResp());
}

[EventSubscribe<RenameFlashFileEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x9427_0")]
internal sealed class RenameFlashFileService : OidbService<RenameFlashFileEventReq, RenameFlashFileEventResp, D9427Req, DEmptyResp>
{
    protected override uint Command => 0x9427;
    protected override uint Service => 0;
    protected override uint Reserved => 1;
    protected override Task<D9427Req> ProcessRequest(RenameFlashFileEventReq request, BotContext context) => Task.FromResult(new D9427Req { FilesetUuid = request.FilesetUuid, Name = new D9427Name { NewName = request.NewName, DisplayName = request.NewName }, Flag = new D9427Flag { Field1 = 0 } });
    protected override Task<RenameFlashFileEventResp> ProcessResponse(DEmptyResp response, BotContext context) => Task.FromResult(new RenameFlashFileEventResp());
}

[EventSubscribe<SendFlashMessageEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x93d7_1")]
internal sealed class SendFlashMessageService : OidbService<SendFlashMessageEventReq, SendFlashMessageEventResp, D93D7Req, DEmptyResp>
{
    protected override uint Command => 0x93d7;
    protected override uint Service => 1;
    protected override async Task<D93D7Req> ProcessRequest(SendFlashMessageEventReq request, BotContext context)
    {
        if (request.UserUin is { } user)
        {
            var friend = await context.CacheContext.ResolveFriend(user) ?? throw new Lagrange.Core.Exceptions.InvalidTargetException(user);
            return new D93D7Req { FilesetUuid = request.FilesetUuid, Target = new D93D7Target { Field1 = 1, Uid = new D93D7Uid { Uid = friend.Uid } } };
        }
        if (request.GroupUin is not { } group || group <= 0 || group > uint.MaxValue) throw new ArgumentException("A valid target is required.");
        return new D93D7Req { FilesetUuid = request.FilesetUuid, Target = new D93D7Target { Field1 = 2, Group = new D93D7Group { GroupId = (uint)group } } };
    }
    protected override Task<SendFlashMessageEventResp> ProcessResponse(DEmptyResp response, BotContext context) => Task.FromResult(new SendFlashMessageEventResp());
}
