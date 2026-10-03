using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class D112AProfileString
{
    [ProtoMember(1)] public uint FieldId { get; set; }
    [ProtoMember(2)] public string Value { get; set; }
}

[ProtoPackable]
internal partial class D112AProfileInt
{
    [ProtoMember(1)] public uint FieldId { get; set; }
    [ProtoMember(2)] public ulong Value { get; set; }
}

[ProtoPackable]
internal partial class D112AReqBody
{
    [ProtoMember(1)] public ulong Uin { get; set; }
    [ProtoMember(2)] public List<D112AProfileString>? StringProfiles { get; set; }
    [ProtoMember(3)] public List<D112AProfileInt>? IntProfiles { get; set; }
}

[ProtoPackable]
internal partial class D112ARespBody { }

[ProtoPackable]
internal partial class D112ASingleReqBody
{
    [ProtoMember(1)] public ulong Uin { get; set; }
    [ProtoMember(2)] public D112AProfileString Profile { get; set; }
}
