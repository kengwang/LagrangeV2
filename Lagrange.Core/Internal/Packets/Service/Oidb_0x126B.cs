using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class D126BReqBody
{
    [ProtoMember(1)] public D126BField1 Field1 { get; set; }
}

[ProtoPackable]
internal partial class D126BField1
{
    [ProtoMember(1)] public string TargetUid { get; set; }
    [ProtoMember(2)] public D126BField2 Field2 { get; set; }
    [ProtoMember(3)] public bool Block { get; set; }
    [ProtoMember(4)] public bool Field4 { get; set; }
}

[ProtoPackable]
internal partial class D126BField2
{
    [ProtoMember(1)] public uint Field1 { get; set; }
    [ProtoMember(2)] public uint Field2 { get; set; }
    [ProtoMember(3)] public D126BField3 Field3 { get; set; }
}

[ProtoPackable]
internal partial class D126BField3
{
    [ProtoMember(1)] public uint Field1 { get; set; }
    [ProtoMember(2)] public uint Field2 { get; set; }
    [ProtoMember(3)] public uint Field3 { get; set; }
}

[ProtoPackable]
internal partial class D126BRespBody { }
