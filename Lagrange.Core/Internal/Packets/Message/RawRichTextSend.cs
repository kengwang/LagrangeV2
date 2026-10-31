using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Message;

[ProtoPackable] internal partial class RawRichTextBody { [ProtoMember(1)] public ReadOnlyMemory<byte> RichText { get; set; } }
[ProtoPackable] internal partial class RawRichTextSend
{
    [ProtoMember(1)] public SendRoutingHead RoutingHead { get; set; } = new();
    [ProtoMember(2)] public SendContentHead ContentHead { get; set; } = new();
    [ProtoMember(3)] public RawRichTextBody MessageBody { get; set; } = new();
    [ProtoMember(4)] public ulong ClientSequence { get; set; }
    [ProtoMember(5)] public uint Random { get; set; }
    [ProtoMember(12)] public SendMessageControl? Control { get; set; }
}
[ProtoPackable] internal partial class RawPttCompatibility { [ProtoMember(4)] public byte[] Ptt { get; set; } = []; }
