using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class PrivateFSDownloadRequest
{
    [ProtoMember(1)] public uint SubCommand { get; set; } = 1200;

    [ProtoMember(2)] public int Field2 { get; set; } = 1;

    [ProtoMember(14)] public PrivateFSDownloadRequestBody Body { get; set; }

    [ProtoMember(101)] public int Field101 { get; set; } = 3;

    [ProtoMember(102)] public int Field102 { get; set; } = 103;

    [ProtoMember(200)] public int Field200 { get; set; } = 1;

    [ProtoMember(99999)] public byte[] Field99999 { get; set; } = [0xc0, 0x85, 0x2c, 0x01];
}

[ProtoPackable]
internal partial class PrivateFSDownloadRequestBody
{
    [ProtoMember(10)] public string ReceiverUid { get; set; }

    [ProtoMember(20)] public string FileUuid { get; set; }

    [ProtoMember(30)] public int Type { get; set; } = 2;

    [ProtoMember(60)] public string FileHash { get; set; }

    [ProtoMember(601)] public int T2 { get; set; }
}

[ProtoPackable]
internal partial class PrivateFSDownloadResponse
{
    [ProtoMember(14)] public PrivateFSDownloadResponseBody Body { get; set; }
}

[ProtoPackable]
internal partial class PrivateFSDownloadResponseBody
{
    [ProtoMember(30)] public PrivateFSDownloadResult Result { get; set; }
}

[ProtoPackable]
internal partial class PrivateFSDownloadResult
{
    [ProtoMember(20)] public string Server { get; set; }

    [ProtoMember(40)] public uint Port { get; set; }

    [ProtoMember(50)] public string Url { get; set; }
}
