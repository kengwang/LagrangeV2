using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Notify;

[ProtoPackable]
internal partial class GroupMute
{
    [ProtoMember(1)] public long GroupUin { get; set; }
    [ProtoMember(2)] public uint? SubType { get; set; }
    [ProtoMember(3)] public uint? Field3 { get; set; }
    [ProtoMember(4)] public string? OperatorUid { get; set; }
    [ProtoMember(5)] public GroupMuteData? Data { get; set; }
}

[ProtoPackable]
internal partial class GroupMuteData
{
    [ProtoMember(1)] public uint? Timestamp { get; set; }
    [ProtoMember(2)] public uint? Type { get; set; }
    [ProtoMember(3)] public GroupMuteState? State { get; set; }
}

[ProtoPackable]
internal partial class GroupMuteState
{
    [ProtoMember(1)] public string TargetUid { get; set; } = string.Empty;
    [ProtoMember(2)] public uint? Duration { get; set; }
}
