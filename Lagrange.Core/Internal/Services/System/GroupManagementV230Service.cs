using Lagrange.Core.Common;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<GetGroupTagsEvent>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x967c_0")]
internal sealed class GetGroupTagsService() : OidbService<GetGroupTagsEvent, GroupTagsResult, GroupTagsRequest, GroupTagsResponse>(0x967c, 0)
{
    protected override Task<GroupTagsRequest> ProcessRequest(GetGroupTagsEvent request, BotContext context) => Task.FromResult(new GroupTagsRequest { GroupClass = request.GroupClass });
    protected override Task<GroupTagsResult> ProcessResponse(GroupTagsResponse response, BotContext context) => Task.FromResult(new GroupTagsResult(response.Tags.Select(x => new BotGroupCategoryTag(x.Id, x.Name, x.Type)).ToArray()));
}
[EventSubscribe<AddGroupTagEvent>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x967d_0")]
internal sealed class AddGroupTagService() : OidbService<AddGroupTagEvent, GroupManagementSuccess, GroupTagsRequest, DEmptyResp>(0x967d, 0)
{
    protected override Task<GroupTagsRequest> ProcessRequest(AddGroupTagEvent request, BotContext context) => Task.FromResult(new GroupTagsRequest { GroupClass = request.GroupClass, Name = request.Name });
    protected override Task<GroupManagementSuccess> ProcessResponse(DEmptyResp response, BotContext context) => Task.FromResult(new GroupManagementSuccess());
}
[EventSubscribe<DeleteGroupTagEvent>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x967e_0")]
internal sealed class DeleteGroupTagService() : OidbService<DeleteGroupTagEvent, GroupManagementSuccess, GroupTagsRequest, DEmptyResp>(0x967e, 0)
{
    protected override Task<GroupTagsRequest> ProcessRequest(DeleteGroupTagEvent request, BotContext context) => Task.FromResult(new GroupTagsRequest { GroupClass = request.GroupClass, Name = request.Name });
    protected override Task<GroupManagementSuccess> ProcessResponse(DEmptyResp response, BotContext context) => Task.FromResult(new GroupManagementSuccess());
}
[EventSubscribe<GetGroupShareLinkEvent>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x9060_3")]
internal sealed class GetGroupShareLinkService() : OidbService<GetGroupShareLinkEvent, GroupShareLinkResult, GroupShareRequest, GroupShareResponse>(0x9060, 3, 1)
{
    protected override Task<GroupShareRequest> ProcessRequest(GetGroupShareLinkEvent request, BotContext context) => Task.FromResult(new GroupShareRequest { Group = new GroupShareTarget { GroupCode = request.GroupId, NeedShortUrl = request.ShortUrl, AdditionalParam = request.AdditionalParam } });
    protected override Task<GroupShareLinkResult> ProcessResponse(GroupShareResponse response, BotContext context)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(response.Group?.Url);
        return Task.FromResult(new GroupShareLinkResult(response.Group.Url));
    }
}
[EventSubscribe<GetAdminPermissionsEvent>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x9164_2")]
internal sealed class GetAdminPermissionsService() : OidbService<GetAdminPermissionsEvent, AdminPermissionsResult, GetAdminPermissionsRequest, GetAdminPermissionsResponse>(0x9164, 2, 1)
{
    protected override Task<GetAdminPermissionsRequest> ProcessRequest(GetAdminPermissionsEvent request, BotContext context) => Task.FromResult(new GetAdminPermissionsRequest { GroupCode = request.GroupId, MemberUins = [request.UserId] });
    protected override Task<AdminPermissionsResult> ProcessResponse(GetAdminPermissionsResponse response, BotContext context) => Task.FromResult(new AdminPermissionsResult(response.GroupCode, response.Members));
}
[EventSubscribe<SetAdminPermissionsEvent>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x9c1c_2")]
internal sealed class SetAdminPermissionsService() : OidbService<SetAdminPermissionsEvent, GroupManagementSuccess, SetAdminPermissionsRequest, DEmptyResp>(0x9c1c, 2)
{
    protected override Task<SetAdminPermissionsRequest> ProcessRequest(SetAdminPermissionsEvent request, BotContext context) => Task.FromResult(new SetAdminPermissionsRequest { GroupCode = request.GroupId, MemberUin = request.UserId, Permissions = [.. request.Permissions] });
    protected override Task<GroupManagementSuccess> ProcessResponse(DEmptyResp response, BotContext context) => Task.FromResult(new GroupManagementSuccess());
}
[EventSubscribe<GetAdminVerifyEvent>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0xfe7_4")]
internal sealed class GetAdminVerifyService() : OidbService<GetAdminVerifyEvent, AdminVerifyResultEvent, GetAdminVerifyRequest, GetAdminVerifyResponse>(0xfe7, 4)
{
    protected override Task<GetAdminVerifyRequest> ProcessRequest(GetAdminVerifyEvent request, BotContext context) => Task.FromResult(new GetAdminVerifyRequest { GroupCode = request.GroupId, Members = [new AdminVerifyIdentity { Uid = request.Uid }] });
    protected override Task<AdminVerifyResultEvent> ProcessResponse(GetAdminVerifyResponse response, BotContext context) => Task.FromResult(new AdminVerifyResultEvent(response.GroupCode, response.Members));
}
[EventSubscribe<SetAdminVerifyEvent>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x904_1")]
internal sealed class SetAdminVerifyService() : OidbService<SetAdminVerifyEvent, SetAdminVerifyResultEvent, SetAdminVerifyRequest, SetAdminVerifyResponse>(0x904, 1, 1)
{
    protected override Task<SetAdminVerifyRequest> ProcessRequest(SetAdminVerifyEvent request, BotContext context) => Task.FromResult(new SetAdminVerifyRequest { GroupCode = request.GroupId, Operation = new AdminVerifyOperation { AdminUins = [request.UserId], Operate = request.Enable ? 1u : 0u } });
    protected override Task<SetAdminVerifyResultEvent> ProcessResponse(SetAdminVerifyResponse response, BotContext context) => Task.FromResult(new SetAdminVerifyResultEvent(response.GroupCode, response.Result?.SuccessfulUins ?? []));
}
