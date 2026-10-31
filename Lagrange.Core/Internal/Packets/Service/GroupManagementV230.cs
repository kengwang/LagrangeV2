using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

[ProtoPackable] internal partial class GroupTagsRequest
{
    [ProtoMember(1)] public uint? GroupClass { get; set; }
    [ProtoMember(2)] public string? Name { get; set; }
}
[ProtoPackable] internal partial class GroupTagsResponse
{
    [ProtoMember(1)] public List<GroupCategoryTag> Tags { get; set; } = [];
}
[ProtoPackable] internal partial class GroupCategoryTag
{
    [ProtoMember(1)] public uint Id { get; set; }
    [ProtoMember(2)] public string Name { get; set; } = string.Empty;
    [ProtoMember(3)] public uint Type { get; set; }
}
[ProtoPackable] internal partial class GroupShareRequest
{
    [ProtoMember(1)] public ulong Type { get; set; } = 2;
    [ProtoMember(12)] public GroupShareTarget Group { get; set; } = new();
}
[ProtoPackable] internal partial class GroupShareTarget
{
    [ProtoMember(1)] public long GroupCode { get; set; }
    [ProtoMember(3)] public string? AdditionalParam { get; set; }
    [ProtoMember(4)] public bool? NeedShortUrl { get; set; }
}
[ProtoPackable] internal partial class GroupShareResponse
{
    [ProtoMember(12)] public GroupShareUrl? Group { get; set; }
}
[ProtoPackable] internal partial class GroupShareUrl
{
    [ProtoMember(1)] public string Url { get; set; } = string.Empty;
}
[ProtoPackable] internal partial class AdminPermissionItem
{
    [ProtoMember(1)] public uint Item { get; set; }
    [ProtoMember(2)] public uint Value { get; set; }
    [ProtoMember(3)] public byte[] Extra { get; set; } = [];
}
[ProtoPackable] internal partial class AdminKickLimit
{
    [ProtoMember(1)] public uint MaxKickPerDay { get; set; }
}
[ProtoPackable] internal partial class SetAdminPermissionsRequest
{
    [ProtoMember(1)] public long GroupCode { get; set; }
    [ProtoMember(2)] public long MemberUin { get; set; }
    [ProtoMember(3)] public List<AdminPermissionItem> Permissions { get; set; } = [];
}
[ProtoPackable] internal partial class GetAdminPermissionsRequest
{
    [ProtoMember(1)] public long GroupCode { get; set; }
    [ProtoMember(2)] public List<long> MemberUins { get; set; } = [];
    [ProtoMember(4)] public AdminPermissionFilter Filter { get; set; } = new();
}
[ProtoPackable] internal partial class AdminPermissionFilter
{
    [ProtoMember(52)] public byte[] Permissions { get; set; } = [1];
}
[ProtoPackable] internal partial class AdminPermissionItems
{
    [ProtoMember(1)] public List<AdminPermissionItem> Items { get; set; } = [];
}
[ProtoPackable] internal partial class AdminPermissionExt
{
    [ProtoMember(52)] public AdminPermissionItems? Permissions { get; set; }
}
[ProtoPackable] internal partial class AdminPermissionMember
{
    [ProtoMember(1)] public long MemberUin { get; set; }
    [ProtoMember(3)] public AdminPermissionExt? Ext { get; set; }
}
[ProtoPackable] internal partial class GetAdminPermissionsResponse
{
    [ProtoMember(1)] public long GroupCode { get; set; }
    [ProtoMember(3)] public List<AdminPermissionMember> Members { get; set; } = [];
}
[ProtoPackable] internal partial class SetAdminVerifyRequest
{
    [ProtoMember(1)] public long GroupCode { get; set; }
    [ProtoMember(2)] public AdminVerifyOperation Operation { get; set; } = new();
}
[ProtoPackable] internal partial class AdminVerifyOperation
{
    [ProtoMember(1)] public List<long> AdminUins { get; set; } = [];
    [ProtoMember(2)] public uint Authority { get; set; } = 1;
    [ProtoMember(3)] public uint? Operate { get; set; }
}
[ProtoPackable] internal partial class AdminVerifyResult
{
    [ProtoMember(1)] public List<long> SuccessfulUins { get; set; } = [];
}
[ProtoPackable] internal partial class SetAdminVerifyResponse
{
    [ProtoMember(1)] public long GroupCode { get; set; }
    [ProtoMember(2)] public AdminVerifyResult? Result { get; set; }
}
[ProtoPackable] internal partial class AdminVerifyIdentity
{
    [ProtoMember(2)] public string Uid { get; set; } = string.Empty;
    [ProtoMember(4)] public long? Uin { get; set; }
}
[ProtoPackable] internal partial class AdminVerifyFilter
{
    [ProtoMember(104)] public uint MemberFlagExt { get; set; } = 1;
}
[ProtoPackable] internal partial class GetAdminVerifyRequest
{
    [ProtoMember(1)] public long GroupCode { get; set; }
    [ProtoMember(2)] public uint Field2 { get; set; } = 3;
    [ProtoMember(3)] public uint? Field3 { get; set; } = 0;
    [ProtoMember(4)] public AdminVerifyFilter Filter { get; set; } = new();
    [ProtoMember(5)] public List<AdminVerifyIdentity> Members { get; set; } = [];
}
[ProtoPackable] internal partial class AdminVerifyMember
{
    [ProtoMember(1)] public AdminVerifyIdentity Identity { get; set; } = new();
    [ProtoMember(104)] public uint? MemberFlagExt { get; set; }
}
[ProtoPackable] internal partial class GetAdminVerifyResponse
{
    [ProtoMember(1)] public long GroupCode { get; set; }
    [ProtoMember(2)] public List<AdminVerifyMember> Members { get; set; } = [];
}
