using Lagrange.Proto;
namespace Lagrange.Core.Internal.Packets.Message;
[ProtoPackable]
internal partial class GroupTemp
{
    [ProtoMember(3)] public long GroupUin { get; set; }
    [ProtoMember(4)] public string ToUid { get; set; } = string.Empty;
}
