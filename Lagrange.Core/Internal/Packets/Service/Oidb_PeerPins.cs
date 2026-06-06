using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class FetchPinsRequest;

[ProtoPackable]
internal partial class FetchPinsResponse
{
    [ProtoMember(1)] public List<FetchPinsResponseFriend>? Friends { get; set; }

    [ProtoMember(3)] public List<FetchPinsResponseGroup>? Groups { get; set; }
}

[ProtoPackable]
internal partial class FetchPinsResponseFriend
{
    [ProtoMember(1)] public string Uid { get; set; }
}

[ProtoPackable]
internal partial class FetchPinsResponseGroup
{
    [ProtoMember(1)] public long Uin { get; set; }
}

[ProtoPackable]
internal partial class SetPinFriendRequest
{
    [ProtoMember(1)] public uint Field1 { get; set; }

    [ProtoMember(2)] public SetPinFriendRequestInfo Info { get; set; }

    [ProtoMember(3)] public uint Field3 { get; set; }
}

[ProtoPackable]
internal partial class SetPinFriendRequestInfo
{
    [ProtoMember(1)] public string FriendUid { get; set; }

    [ProtoMember(400)] public SetPinField400 Field400 { get; set; }
}

[ProtoPackable]
internal partial class SetPinGroupRequest
{
    [ProtoMember(1)] public uint Field1 { get; set; }

    [ProtoMember(2)] public SetPinGroupRequestInfo Info { get; set; }

    [ProtoMember(3)] public uint Field3 { get; set; }
}

[ProtoPackable]
internal partial class SetPinGroupRequestInfo
{
    [ProtoMember(2)] public long GroupUin { get; set; }

    [ProtoMember(400)] public SetPinField400 Field400 { get; set; }
}

[ProtoPackable]
internal partial class SetPinField400
{
    [ProtoMember(1)] public uint Field1 { get; set; }

    [ProtoMember(2)] public byte[] Timestamp { get; set; }
}

[ProtoPackable]
internal partial class SetPinResponse;
