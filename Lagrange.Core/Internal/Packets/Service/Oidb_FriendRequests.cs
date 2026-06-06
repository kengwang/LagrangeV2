using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class FetchFriendRequestsRequest
{
    [ProtoMember(1)] public int Field1 { get; set; }

    [ProtoMember(3)] public int Field3 { get; set; }

    [ProtoMember(4)] public string SelfUid { get; set; }

    [ProtoMember(5)] public int Field5 { get; set; }

    [ProtoMember(6)] public int Field6 { get; set; }

    [ProtoMember(8)] public int Field8 { get; set; }

    [ProtoMember(9)] public int Field9 { get; set; }

    [ProtoMember(12)] public int Field12 { get; set; }

    [ProtoMember(22)] public int Field22 { get; set; }
}

[ProtoPackable]
internal partial class FetchFriendRequestsResponse
{
    [ProtoMember(3)] public FetchFriendRequestsResponseInfo Info { get; set; }
}

[ProtoPackable]
internal partial class FetchFriendRequestsResponseInfo
{
    [ProtoMember(7)] public List<FetchFriendRequestsResponseRequest> Requests { get; set; } = [];
}

[ProtoPackable]
internal partial class FetchFriendRequestsResponseRequest
{
    [ProtoMember(1)] public string TargetUid { get; set; }

    [ProtoMember(2)] public string SourceUid { get; set; }

    [ProtoMember(3)] public uint State { get; set; }

    [ProtoMember(4)] public uint Timestamp { get; set; }

    [ProtoMember(5)] public string Comment { get; set; }

    [ProtoMember(6)] public string Source { get; set; }
}

[ProtoPackable]
internal partial class SetFriendRequestRequest
{
    [ProtoMember(1)] public uint Accept { get; set; }

    [ProtoMember(2)] public string TargetUid { get; set; }
}

[ProtoPackable]
internal partial class SetFriendRequestResponse;
