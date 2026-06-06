using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class GroupSetAdminRequest
{
    [ProtoMember(1)] public long GroupUin { get; set; }

    [ProtoMember(2)] public string Uid { get; set; }

    [ProtoMember(3)] public bool IsAdmin { get; set; }
}

[ProtoPackable]
internal partial class GroupSetAdminResponse
{
    [ProtoMember(1)] public string? Success { get; set; }
}

[ProtoPackable]
internal partial class GroupMuteMemberRequest
{
    [ProtoMember(1)] public long GroupUin { get; set; }

    [ProtoMember(2)] public uint Type { get; set; }

    [ProtoMember(3)] public GroupMuteMemberRequestBody Body { get; set; }
}

[ProtoPackable]
internal partial class GroupMuteMemberRequestBody
{
    [ProtoMember(1)] public string TargetUid { get; set; }

    [ProtoMember(2)] public uint Duration { get; set; }
}

[ProtoPackable]
internal partial class GroupMuteMemberResponse
{
    [ProtoMember(2)] public string? Success { get; set; }
}

[ProtoPackable]
internal partial class GroupMuteGlobalRequest
{
    [ProtoMember(1)] public long GroupUin { get; set; }

    [ProtoMember(2)] public GroupMuteGlobalState State { get; set; }
}

[ProtoPackable]
internal partial class GroupMuteGlobalState
{
    [ProtoMember(17)] public uint? S { get; set; }
}

[ProtoPackable]
internal partial class GroupMuteGlobalResponse
{
    [ProtoMember(1)] public long GroupUin { get; set; }

    [ProtoMember(2)] public string? ErrorMsg { get; set; }
}

[ProtoPackable]
internal partial class GroupKickMemberRequest
{
    [ProtoMember(1)] public long GroupUin { get; set; }

    [ProtoMember(3)] public string TargetUid { get; set; }

    [ProtoMember(4)] public bool RejectAddRequest { get; set; }

    [ProtoMember(5)] public string Reason { get; set; } = string.Empty;
}

[ProtoPackable]
internal partial class GroupKickMemberResponse
{
    [ProtoMember(1)] public long GroupUin { get; set; }

    [ProtoMember(2)] public string? ErrorMsg { get; set; }
}
