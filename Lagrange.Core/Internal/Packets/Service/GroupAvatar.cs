using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

[ProtoPackable]
internal partial class GroupAvatarExtraField3 { [ProtoMember(1)] public uint Field1 { get; set; } }
[ProtoPackable]
internal partial class GroupAvatarExtra { [ProtoMember(1)] public uint Type { get; set; } [ProtoMember(2)] public uint GroupUin { get; set; } [ProtoMember(3)] public GroupAvatarExtraField3 Field3 { get; set; } = new(); [ProtoMember(5)] public uint Field5 { get; set; } [ProtoMember(6)] public uint Field6 { get; set; } }
