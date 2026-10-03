using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

#pragma warning disable CS8618

[ProtoPackable] internal partial class D9154Req { [ProtoMember(1)] public int Field1 { get; set; } [ProtoMember(2)] public int Field2 { get; set; } [ProtoMember(3)] public int Field3 { get; set; } }
[ProtoPackable] internal partial class D9154Url { [ProtoMember(1)] public string? BaseUrl { get; set; } [ProtoMember(2)] public string? AdvUrl { get; set; } }
[ProtoPackable] internal partial class D9154Emoji { [ProtoMember(1)] public string? Sid { get; set; } [ProtoMember(2)] public string? Description { get; set; } [ProtoMember(3)] public string? EmCode { get; set; } [ProtoMember(4)] public int CategoryId { get; set; } [ProtoMember(8)] public D9154Url? Url { get; set; } [ProtoMember(9)] public List<string>? Aliases { get; set; } }
[ProtoPackable] internal partial class D9154List { [ProtoMember(1)] public string? PackName { get; set; } [ProtoMember(2)] public List<D9154Emoji>? Emojis { get; set; } }
[ProtoPackable] internal partial class D9154Content { [ProtoMember(1)] public List<D9154List>? Lists { get; set; } }
[ProtoPackable] internal partial class D9154MagicList { [ProtoMember(2)] public List<D9154Emoji>? Emojis { get; set; } }
[ProtoPackable] internal partial class D9154Magic { [ProtoMember(1)] public D9154MagicList? List { get; set; } }
[ProtoPackable] internal partial class D9154Resp { [ProtoMember(2)] public D9154Content? Common { get; set; } [ProtoMember(3)] public D9154Content? SpecialBig { get; set; } [ProtoMember(4)] public D9154Magic? Magic { get; set; } }
