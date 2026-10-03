using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Notify;

[ProtoPackable]
internal partial class GroupNameChange
{
    [ProtoMember(2)] public string Name { get; set; } = string.Empty;
}
