using Lagrange.Core.Events;
using Lagrange.Core.Common.Response;

namespace Lagrange.Core.Internal.Events.System;

internal sealed class GetGroupTagsEvent(uint groupClass) : ProtocolEvent { public uint GroupClass { get; } = groupClass; }
internal sealed class AddGroupTagEvent(uint groupClass, string name) : ProtocolEvent { public uint GroupClass { get; } = groupClass; public string Name { get; } = name; }
internal sealed class DeleteGroupTagEvent(uint groupClass, string name) : ProtocolEvent { public uint GroupClass { get; } = groupClass; public string Name { get; } = name; }
internal sealed class GroupTagsResult(IReadOnlyList<BotGroupCategoryTag> tags) : ProtocolEvent { public IReadOnlyList<BotGroupCategoryTag> Tags { get; } = tags; }
internal sealed class GroupManagementSuccess : ProtocolEvent;
internal sealed class GetGroupShareLinkEvent(long groupId, bool shortUrl, string additionalParam) : ProtocolEvent
{ public long GroupId { get; } = groupId; public bool ShortUrl { get; } = shortUrl; public string AdditionalParam { get; } = additionalParam; }
internal sealed class GroupShareLinkResult(string url) : ProtocolEvent { public string Url { get; } = url; }
internal sealed class GetAdminPermissionsEvent(long groupId, long userId) : ProtocolEvent { public long GroupId { get; } = groupId; public long UserId { get; } = userId; }
internal sealed class AdminPermissionsResult(long groupId, IReadOnlyList<Lagrange.Core.Internal.Packets.Service.AdminPermissionMember> members) : ProtocolEvent
{ public long GroupId { get; } = groupId; public IReadOnlyList<Lagrange.Core.Internal.Packets.Service.AdminPermissionMember> Members { get; } = members; }
internal sealed class SetAdminPermissionsEvent(long groupId, long userId, IReadOnlyList<Lagrange.Core.Internal.Packets.Service.AdminPermissionItem> permissions) : ProtocolEvent
{ public long GroupId { get; } = groupId; public long UserId { get; } = userId; public IReadOnlyList<Lagrange.Core.Internal.Packets.Service.AdminPermissionItem> Permissions { get; } = permissions; }
internal sealed class GetAdminVerifyEvent(long groupId, string uid) : ProtocolEvent { public long GroupId { get; } = groupId; public string Uid { get; } = uid; }
internal sealed class AdminVerifyResultEvent(long groupId, IReadOnlyList<Lagrange.Core.Internal.Packets.Service.AdminVerifyMember> members) : ProtocolEvent
{ public long GroupId { get; } = groupId; public IReadOnlyList<Lagrange.Core.Internal.Packets.Service.AdminVerifyMember> Members { get; } = members; }
internal sealed class SetAdminVerifyEvent(long groupId, long userId, bool enable) : ProtocolEvent { public long GroupId { get; } = groupId; public long UserId { get; } = userId; public bool Enable { get; } = enable; }
internal sealed class SetAdminVerifyResultEvent(long groupId, IReadOnlyList<long> successfulUins) : ProtocolEvent { public long GroupId { get; } = groupId; public IReadOnlyList<long> SuccessfulUins { get; } = successfulUins; }
