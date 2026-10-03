using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Notify;

[ProtoPackable]
internal partial class GroupAdmin
{
    [ProtoMember(1)] public long GroupUin { get; set; }
    [ProtoMember(2)] public uint? Flag { get; set; }
    [ProtoMember(3)] public bool? IsPromote { get; set; }
    [ProtoMember(4)] public GroupAdminBody? Body { get; set; }
}

[ProtoPackable]
internal partial class GroupAdminBody
{
    [ProtoMember(1)] public GroupAdminExtra? ExtraDisable { get; set; }
    [ProtoMember(2)] public GroupAdminExtra? ExtraEnable { get; set; }
}

[ProtoPackable]
internal partial class GroupAdminExtra
{
    [ProtoMember(1)] public string AdminUid { get; set; } = string.Empty;
    [ProtoMember(2)] public bool? IsPromote { get; set; }
}
