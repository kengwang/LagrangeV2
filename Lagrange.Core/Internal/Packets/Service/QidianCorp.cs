using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class QidianCorpInfoRequest
{
    [ProtoMember(1)] public uint Uin { get; set; }
}

[ProtoPackable]
internal partial class QidianCorpInfoResponse
{
    [ProtoMember(2)] public string CorpName { get; set; }
    [ProtoMember(3)] public string Intro { get; set; }
    [ProtoMember(4)] public string Website { get; set; }
    [ProtoMember(5)] public string Slogan { get; set; }
    [ProtoMember(6)] public string Address { get; set; }
    [ProtoMember(7)] public string Phone { get; set; }
    [ProtoMember(8)] public string Email { get; set; }
}
