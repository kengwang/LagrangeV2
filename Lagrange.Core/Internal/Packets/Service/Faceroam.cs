using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class FaceroamInner
{
    [ProtoMember(1)] public uint Field1 { get; set; }
    [ProtoMember(2)] public string OsVersion { get; set; }
    [ProtoMember(3)] public string QqVersion { get; set; }
}

[ProtoPackable]
internal partial class FaceroamBody
{
    [ProtoMember(1)] public string EmojiId { get; set; }
}

[ProtoPackable]
internal partial class FaceroamRequest
{
    [ProtoMember(1)] public FaceroamInner Inner { get; set; }
    [ProtoMember(2)] public ulong Uin { get; set; }
    [ProtoMember(3)] public uint Field3 { get; set; }
    [ProtoMember(5)] public FaceroamBody Body { get; set; }
    [ProtoMember(6)] public uint Field6 { get; set; }
}

[ProtoPackable]
internal partial class FaceroamResponseItem
{
    [ProtoMember(1)] public List<string>? FaceIds { get; set; }
    [ProtoMember(4)] public uint TotalCount { get; set; }
}

[ProtoPackable]
internal partial class FaceroamResponse
{
    [ProtoMember(1)] public uint RetCode { get; set; }
    [ProtoMember(2)] public string? Message { get; set; }
    [ProtoMember(4)] public FaceroamResponseItem? Item { get; set; }
}

[ProtoPackable]
internal partial class D902EEmoji
{
    [ProtoMember(1)] public string EmojiId { get; set; }
    [ProtoMember(2)] public string Md5 { get; set; }
}

[ProtoPackable]
internal partial class D902EBody
{
    [ProtoMember(1)] public uint Field1 { get; set; }
    [ProtoMember(2)] public string OsVersion { get; set; }
    [ProtoMember(3)] public uint OpType { get; set; }
    [ProtoMember(4)] public List<D902EEmoji>? Emojis { get; set; }
    [ProtoMember(5)] public D902EModifyEntry? Entry { get; set; }
    [ProtoMember(12)] public uint Field12 { get; set; }
}

[ProtoPackable]
internal partial class D902EModifyEntry
{
    [ProtoMember(1)] public D902EEmoji Emoji { get; set; }
    [ProtoMember(2)] public string Description { get; set; }
}

[ProtoPackable]
internal partial class D902EResponseEntry
{
    [ProtoMember(1)] public D902EEmoji? Emoji { get; set; }
    [ProtoMember(2)] public string? LegacyDescription { get; set; }
    [ProtoMember(3)] public string? Description { get; set; }
}

[ProtoPackable]
internal partial class D902EResponse
{
    [ProtoMember(1)] public uint RetCode { get; set; }
    [ProtoMember(2)] public string? ErrorMessage { get; set; }
    [ProtoMember(4)] public List<D902EResponseEntry>? Entries { get; set; }
}

[ProtoPackable]
internal partial class D902FEnv
{
    [ProtoMember(1)] public uint Field1 { get; set; }
    [ProtoMember(2)] public string OsVersion { get; set; }
    [ProtoMember(3)] public string BuildVersion { get; set; }
}

[ProtoPackable]
internal partial class D902FBody
{
    [ProtoMember(1)] public D902FEnv Env { get; set; }
    [ProtoMember(2)] public string EmojiId { get; set; }
    [ProtoMember(3)] public uint Position { get; set; }
}
