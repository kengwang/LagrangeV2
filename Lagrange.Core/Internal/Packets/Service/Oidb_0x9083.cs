using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class D9083ReqBody
{
    [ProtoMember(2)] public ulong GroupId { get; set; }
    [ProtoMember(3)] public ulong Sequence { get; set; }
    [ProtoMember(4)] public uint EmojiType { get; set; }
    [ProtoMember(5)] public string EmojiId { get; set; }
    [ProtoMember(6)] public string Cookie { get; set; }
    [ProtoMember(7)] public uint Field7 { get; set; }
    [ProtoMember(8)] public uint Count { get; set; }
}

[ProtoPackable]
internal partial class D9083RespBody
{
    [ProtoMember(1)] public List<D9083User>? Users { get; set; }
    [ProtoMember(2)] public string? Cookie { get; set; }
    [ProtoMember(3)] public bool IsLast { get; set; }
}

[ProtoPackable]
internal partial class D9083User
{
    [ProtoMember(1)] public ulong Uin { get; set; }
    [ProtoMember(2)] public string Nickname { get; set; }
    [ProtoMember(3)] public string HeadUrl { get; set; }
}
