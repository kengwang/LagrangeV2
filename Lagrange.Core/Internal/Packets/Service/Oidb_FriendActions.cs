using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class FriendLikeRequest
{
    [ProtoMember(11)] public string TargetUid { get; set; }

    [ProtoMember(12)] public uint Field2 { get; set; }

    [ProtoMember(13)] public uint Count { get; set; }
}

[ProtoPackable]
internal partial class FriendLikeResponse;

[ProtoPackable]
internal partial class DeleteFriendRequest
{
    [ProtoMember(1)] public DeleteFriendRequestBody Body { get; set; } = new();
}

[ProtoPackable]
internal partial class DeleteFriendRequestBody
{
    [ProtoMember(1)] public string TargetUid { get; set; }

    [ProtoMember(2)] public DeleteFriendRequestBodyField2 Field2 { get; set; } = new();

    [ProtoMember(3)] public bool Block { get; set; }

    [ProtoMember(4)] public bool Field4 { get; set; }
}

[ProtoPackable]
internal partial class DeleteFriendRequestBodyField2
{
    [ProtoMember(1)] public uint Field1 { get; set; } = 130;

    [ProtoMember(2)] public uint Field2 { get; set; } = 109;

    [ProtoMember(3)] public DeleteFriendRequestBodyField3 Field3 { get; set; } = new();
}

[ProtoPackable]
internal partial class DeleteFriendRequestBodyField3
{
    [ProtoMember(1)] public uint Field1 { get; set; } = 8;

    [ProtoMember(2)] public uint Field2 { get; set; } = 8;

    [ProtoMember(3)] public uint Field3 { get; set; } = 50;
}

[ProtoPackable]
internal partial class DeleteFriendResponse;
