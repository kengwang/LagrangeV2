using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

[ProtoPackable]
internal partial class D7EDReqUser
{
    [ProtoMember(1)] public string Uid { get; set; } = string.Empty;
}

[ProtoPackable]
internal partial class D7EDReqBody
{
    [ProtoMember(1)] public List<string> TargetUids { get; set; } = [];
    [ProtoMember(2)] public uint Basic { get; set; }
    [ProtoMember(3)] public uint Vote { get; set; }
    [ProtoMember(4)] public uint Favorite { get; set; }
    [ProtoMember(5)] public uint UserProfile { get; set; }
    [ProtoMember(6)] public uint Start { get; set; }
    [ProtoMember(7)] public uint Limit { get; set; }
}

[ProtoPackable]
internal partial class D7EDUserInfo
{
    [ProtoMember(1)] public string Uid { get; set; } = string.Empty;
    [ProtoMember(2)] public long LatestTime { get; set; }
    [ProtoMember(3)] public uint Count { get; set; }
    [ProtoMember(8)] public string Nickname { get; set; } = string.Empty;
    [ProtoMember(14)] public bool IsFriend { get; set; }
}

[ProtoPackable]
internal partial class D7EDVoteInfo
{
    [ProtoMember(1)] public uint TotalCount { get; set; }
    [ProtoMember(2)] public uint NewCount { get; set; }
    [ProtoMember(5)] public List<D7EDUserInfo> Users { get; set; } = [];
}

[ProtoPackable]
internal partial class D7EDUserLikeInfo
{
    [ProtoMember(1)] public string Uid { get; set; } = string.Empty;
    [ProtoMember(2)] public long Time { get; set; }
    [ProtoMember(4)] public D7EDVoteInfo? VoteInfo { get; set; }
}

[ProtoPackable]
internal partial class D7EDRespBody
{
    [ProtoMember(1)] public List<D7EDUserLikeInfo> UserLikeInfos { get; set; } = [];
}
