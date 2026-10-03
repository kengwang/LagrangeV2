using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class D9084ReqBody
{
    [ProtoMember(2)] public ulong GroupId { get; set; }
    [ProtoMember(3)] public ulong Sequence { get; set; }
    [ProtoMember(4)] public string EmojiId { get; set; }
    [ProtoMember(5)] public uint EmojiType { get; set; }
    [ProtoMember(6)] public string Cookie { get; set; }
    [ProtoMember(8)] public uint Count { get; set; }
    [ProtoMember(12)] public uint Field12 { get; set; }
}

[ProtoPackable]
internal partial class D9084RespBody
{
    [ProtoMember(1)] public uint Field1 { get; set; }
    [ProtoMember(2)] public List<D9084Entry>? Entries { get; set; }
}

[ProtoPackable]
internal partial class D9084Entry
{
    [ProtoMember(1)] public ulong LastReactionTime { get; set; }
    [ProtoMember(2)] public uint Count { get; set; }
    [ProtoMember(3)] public uint EmojiType { get; set; }
    [ProtoMember(4)] public string EmojiId { get; set; }
}
