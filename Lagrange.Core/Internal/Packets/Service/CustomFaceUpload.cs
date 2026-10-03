using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

[ProtoPackable]
internal partial class BdhExpressionRoamInner
{
    [ProtoMember(1)] public uint Field1 { get; set; }
    [ProtoMember(2)] public ulong Uin { get; set; }
    [ProtoMember(3)] public uint Field3 { get; set; }
    [ProtoMember(4)] public byte[] Md5 { get; set; } = [];
    [ProtoMember(5)] public uint FileSize { get; set; }
    [ProtoMember(7)] public uint Field7 { get; set; }
    [ProtoMember(8)] public uint Field8 { get; set; }
    [ProtoMember(9)] public uint Field9 { get; set; }
    [ProtoMember(13)] public string Version { get; set; } = string.Empty;
    [ProtoMember(16)] public uint Field16 { get; set; }
}

[ProtoPackable]
internal partial class BdhExpressionRoamTailInner { [ProtoMember(1)] public uint Field1 { get; set; } [ProtoMember(2)] public uint Field2 { get; set; } [ProtoMember(3)] public string Field3 { get; set; } = string.Empty; }
[ProtoPackable]
internal partial class BdhExpressionRoamTail { [ProtoMember(1)] public BdhExpressionRoamTailInner Inner { get; set; } = new(); [ProtoMember(2)] public uint Field2 { get; set; } }
[ProtoPackable]
internal partial class BdhExpressionRoamRequest
{
    [ProtoMember(1)] public uint Field1 { get; set; }
    [ProtoMember(2)] public uint Field2 { get; set; }
    [ProtoMember(3)] public BdhExpressionRoamInner Inner { get; set; } = new();
    [ProtoMember(7)] public uint Field7 { get; set; }
    [ProtoMember(1001)] public BdhExpressionRoamTail Tail { get; set; } = new();
}
[ProtoPackable]
internal partial class BdhExpressionRoamResponseInner { [ProtoMember(8)] public byte[] Token { get; set; } = []; }
[ProtoPackable]
internal partial class BdhExpressionRoamResponse { [ProtoMember(1)] public uint Field1 { get; set; } [ProtoMember(2)] public uint Field2 { get; set; } [ProtoMember(3)] public BdhExpressionRoamResponseInner? Inner { get; set; } }

[ProtoPackable]
internal partial class FavEmojiHighwayBaseHead { [ProtoMember(1)] public uint Version { get; set; } [ProtoMember(2)] public string Uin { get; set; } = string.Empty; [ProtoMember(3)] public string Command { get; set; } = string.Empty; [ProtoMember(4)] public uint Sequence { get; set; } [ProtoMember(5)] public uint RetryTimes { get; set; } [ProtoMember(6)] public ulong FileSize { get; set; } [ProtoMember(7)] public uint DataFlag { get; set; } [ProtoMember(8)] public uint CommandId { get; set; } }
[ProtoPackable]
internal partial class FavEmojiHighwaySegHead { [ProtoMember(1)] public uint ServiceId { get; set; } [ProtoMember(2)] public ulong FileSize { get; set; } [ProtoMember(3)] public ulong DataOffset { get; set; } [ProtoMember(4)] public ulong DataLength { get; set; } [ProtoMember(6)] public byte[] ServiceTicket { get; set; } = []; [ProtoMember(8)] public byte[] Md5 { get; set; } = []; [ProtoMember(9)] public byte[] FileMd5 { get; set; } = []; }
[ProtoPackable]
internal partial class FavEmojiIdWrap { [ProtoMember(1)] public string EmojiId { get; set; } = string.Empty; }
[ProtoPackable]
internal partial class FavEmojiHighwayHead { [ProtoMember(1)] public FavEmojiHighwayBaseHead BaseHead { get; set; } = new(); [ProtoMember(2)] public FavEmojiHighwaySegHead SegHead { get; set; } = new(); [ProtoMember(3)] public FavEmojiIdWrap EmojiIdWrap { get; set; } = new(); [ProtoMember(4)] public uint Field4 { get; set; } [ProtoMember(8)] public uint Field8 { get; set; } }
