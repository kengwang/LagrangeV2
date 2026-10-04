using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Notify;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class NotifyMessageBody
{
    [ProtoMember(1)] public uint NotifyType { get; set; }

    [ProtoMember(4)] public long GroupUin { get; set; }

    [ProtoMember(5)] public byte[]? EventParam { get; set; }

    [ProtoMember(11)] public GroupRecall Recall { get; set; }

    [ProtoMember(13)] public uint SubType { get; set; }

    [ProtoMember(21)] public string OperatorUid { get; set; }

    [ProtoMember(26)] public GeneralGrayTipInfo GeneralGrayTip { get; set; }

    [ProtoMember(33)] public EssenceMessage EssenceMessage;

    [ProtoMember(37)] public ulong MsgSequence { get; set; }

    [ProtoMember(39)] public uint Field39 { get; set; }

    [ProtoMember(40)] public GroupRecallNudge GroupRecallNudge { get; set; }

    [ProtoMember(44)] public GroupReactionData0 Reaction { get; set; }

    [ProtoMember(50)] public ulong TipsSeqId { get; set; }
}

[ProtoPackable]
internal partial class GeneralGrayTipInfo
{
    [ProtoMember(1)] public ulong BusiType { get; set; }

    [ProtoMember(2)] public ulong BusiId { get; set; }

    [ProtoMember(3)] public uint CtrlFlag { get; set; }

    [ProtoMember(4)] public uint C2CType { get; set; }

    [ProtoMember(5)] public uint ServiceType { get; set; }

    [ProtoMember(6)] public ulong TemplId { get; set; }

    [ProtoMember(7)] public List<TemplParam> MsgTemplParam { get; set; }

    [ProtoMember(8)] public string Content { get; set; }

    [ProtoMember(10)] public ulong TipsSeqId { get; set; }

    [ProtoMember(100)] public GrayTipMsgInfo MsgInfo { get; set; }
}

[ProtoPackable]
internal partial class GrayTipMsgInfo
{
    [ProtoMember(6)] public ulong Sequence { get; set; }
}

[ProtoPackable]
internal partial class TemplParam
{
    [ProtoMember(1)] public string Name { get; set; }

    [ProtoMember(2)] public string Value { get; set; }
}

[ProtoPackable]
internal partial class FriendRecallPokeInfo
{
    [ProtoMember(1)] public string SelfUid { get; set; }

    [ProtoMember(2)] public string PeerUid { get; set; }

    [ProtoMember(3)] public string OperatorUid { get; set; }

    [ProtoMember(4)] public ulong BusiId { get; set; }

    [ProtoMember(5)] public ulong TipsSeqId { get; set; }
}

[ProtoPackable]
internal partial class FriendRemarkChangedNotify
{
    [ProtoMember(1)] public FriendRemarkChangedValue Change { get; set; }
    [ProtoMember(3)] public uint UpdateTime { get; set; }
    [ProtoMember(4)] public string Nickname { get; set; }
    [ProtoMember(5)] public uint ChangeType { get; set; }
}

[ProtoPackable]
internal partial class FriendRemarkChangedValue
{
    [ProtoMember(1)] public FriendRemarkChangedTarget Target { get; set; }
    [ProtoMember(2)] public string Remark { get; set; }
}

[ProtoPackable]
internal partial class FriendRemarkChangedTarget
{
    [ProtoMember(3)] public long Uin { get; set; }
    [ProtoMember(7)] public string Uid { get; set; }
}

[ProtoPackable]
internal partial class InputStatusNotify
{
    [ProtoMember(1)] public string FromUid { get; set; }
    [ProtoMember(2)] public string ToUid { get; set; }
    [ProtoMember(3)] public InputStatusNotifyItem NotifyItem { get; set; }
}

[ProtoPackable]
internal partial class InputStatusNotifyItem
{
    [ProtoMember(4)] public uint EventType { get; set; }
}

[ProtoPackable]
internal partial class OnlineDeviceNotify
{
    [ProtoMember(1)] public uint AppId { get; set; }
    [ProtoMember(2)] public uint InstanceId { get; set; }
    [ProtoMember(3)] public uint ClientType { get; set; }
    [ProtoMember(4)] public uint Platform { get; set; }
    [ProtoMember(5)] public List<OnlineDeviceNotifyItem> Devices { get; set; } = [];
}

[ProtoPackable]
internal partial class OnlineDeviceNotifyItem
{
    [ProtoMember(1)] public uint AppId { get; set; }
    [ProtoMember(2)] public uint InstanceId { get; set; }
    [ProtoMember(3)] public uint ClientType { get; set; }
    [ProtoMember(4)] public uint Platform { get; set; }
    [ProtoMember(5)] public string DeviceName { get; set; }
}

[ProtoPackable]
internal partial class GroupSpecialTitleChange
{
    [ProtoMember(2)] public string TipText { get; set; }
    [ProtoMember(5)] public long MemberUin { get; set; }
}

[ProtoPackable]
internal partial class ProfileLikeTip
{
    [ProtoMember(1)] public int MsgType { get; set; }
    [ProtoMember(2)] public int SubType { get; set; }
    [ProtoMember(203)] public ProfileLikeSubTip Content { get; set; }
}

[ProtoPackable]
internal partial class ProfileLikeSubTip
{
    [ProtoMember(14)] public ProfileLikeMsg Msg { get; set; }
}

[ProtoPackable]
internal partial class ProfileLikeMsg
{
    [ProtoMember(1)] public int Times { get; set; }
    [ProtoMember(2)] public int Time { get; set; }
    [ProtoMember(3)] public ProfileLikeDetail Detail { get; set; }
}

[ProtoPackable]
internal partial class ProfileLikeDetail
{
    [ProtoMember(1)] public string Text { get; set; }
    [ProtoMember(3)] public long Uin { get; set; }
    [ProtoMember(5)] public string Nickname { get; set; }
}

[ProtoPackable]
internal partial class NewFriend
{
    [ProtoMember(1)] public uint Field1 { get; set; }
    [ProtoMember(2)] public NewFriendInfo Info { get; set; }
}

[ProtoPackable]
internal partial class NewFriendInfo
{
    [ProtoMember(1)] public string Uid { get; set; }
    [ProtoMember(2)] public uint Field2 { get; set; }
    [ProtoMember(3)] public uint Time { get; set; }
    [ProtoMember(4)] public string Message { get; set; }
    [ProtoMember(5)] public string Nickname { get; set; }
    [ProtoMember(6)] public uint Field6 { get; set; }
    [ProtoMember(7)] public uint Field7 { get; set; }
    [ProtoMember(9)] public string ToUid { get; set; }
}

[ProtoPackable]
internal partial class SelfJoinInGroup
{
    [ProtoMember(1)] public long GroupUin { get; set; }
    [ProtoMember(2)] public uint Field2 { get; set; }
    [ProtoMember(3)] public string OperatorUid { get; set; }
    [ProtoMember(4)] public uint Field4 { get; set; }
    [ProtoMember(6)] public uint Field6 { get; set; }
    [ProtoMember(7)] public string Field7 { get; set; }
}
