using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class DDC2Req
{
    [ProtoMember(1)] public DDC2AppInfo AppInfo { get; set; }
    [ProtoMember(2)] public DDC2Meta Meta { get; set; }
}

[ProtoPackable]
internal partial class DDC2AppInfo
{
    [ProtoMember(1)] public uint AppId { get; set; }
    [ProtoMember(2)] public uint Field2 { get; set; }
    [ProtoMember(3)] public uint Field3 { get; set; }
    [ProtoMember(5)] public DDC2Field5 Field5 { get; set; }
    [ProtoMember(11)] public uint TargetId { get; set; }
    [ProtoMember(12)] public DDC2Content Content { get; set; }
}

[ProtoPackable]
internal partial class DDC2Field5 { [ProtoMember(1)] public uint Field1 { get; set; } }

[ProtoPackable]
internal partial class DDC2Content
{
    [ProtoMember(1)] public uint Flag { get; set; }
    [ProtoMember(10)] public string Title { get; set; }
    [ProtoMember(11)] public string Description { get; set; }
    [ProtoMember(12)] public string Summary { get; set; }
    [ProtoMember(13)] public string JumpUrl { get; set; }
    [ProtoMember(14)] public string PreviewUrl { get; set; }
}

[ProtoPackable]
internal partial class DDC2Meta
{
    [ProtoMember(1)] public uint PeerType { get; set; }
    [ProtoMember(2)] public uint TargetId { get; set; }
}
