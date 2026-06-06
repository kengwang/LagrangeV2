using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class FaceRoamRequest
{
    [ProtoMember(1)] public FaceRoamPlatInfo? Comm { get; set; }

    [ProtoMember(2)] public long SelfUin { get; set; }

    [ProtoMember(3)] public uint SubCmd { get; set; }

    [ProtoMember(6)] public uint Field6 { get; set; }
}

[ProtoPackable]
internal partial class FaceRoamPlatInfo
{
    [ProtoMember(1)] public uint ImPlat { get; set; }

    [ProtoMember(2)] public string? OsVersion { get; set; }

    [ProtoMember(3)] public string? QVersion { get; set; }
}

[ProtoPackable]
internal partial class FaceRoamResponse
{
    [ProtoMember(1)] public uint RetCode { get; set; }

    [ProtoMember(2)] public string ErrMsg { get; set; }

    [ProtoMember(4)] public FaceRoamUserInfo UserInfo { get; set; }
}

[ProtoPackable]
internal partial class FaceRoamUserInfo
{
    [ProtoMember(1)] public List<string> FileName { get; set; } = [];

    [ProtoMember(3)] public string Bid { get; set; }
}
