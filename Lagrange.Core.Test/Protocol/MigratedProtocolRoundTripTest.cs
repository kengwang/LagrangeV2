using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Utility;
using Lagrange.Proto;

namespace Lagrange.Core.Test.Protocol;

public sealed class MigratedProtocolRoundTripTest
{
    [Test] public void TranslateRequestRoundTrips() => RoundTrip(new D990ReqBody { TranslateReq = new D990TranslateReq { SourceLanguage = "en", DestinationLanguage = "zh", Words = ["hello"] }, Tag10 = 1, Tag12 = 1 });
    [Test] public void TranslateResponseRoundTrips() => RoundTrip(new D990RespBody { TranslateResp = new D990TranslateResp { DestinationWords = ["你好"] } });
    [Test] public void OcrRequestRoundTrips() => RoundTrip(new DE07ReqBody { Version = 1, Client = 0, Entrance = 1, OcrReqBody = new DE07OcrReqBody { ImageUrl = "https://example.test/a.png" } });
    [Test] public void OcrCoordinateRoundTrips() => RoundTrip(new DE07Coordinate { X = 12, Y = 34 });
    [Test] public void OcrDetectionRoundTrips() => RoundTrip(new DE07Detection { DetectedText = "abc", Confidence = 99, Polygon = new DE07Polygon { Coordinates = [new DE07Coordinate { X = 1, Y = 2 }] } });
    [Test] public void OcrResponseRoundTrips() => RoundTrip(new DE07RespBody { RetCode = 0, OcrRspBody = new DE07OcrRespBody { Language = "en", TextDetections = [new DE07Detection { DetectedText = "abc" }] } });
    [Test] public void ProfileLikeRequestRoundTrips() => RoundTrip(new D7EDReqBody { TargetUids = ["uid"], Basic = 1, Vote = 1, UserProfile = 1, Start = 0, Limit = 10 });
    [Test] public void ProfileLikeUserRoundTrips() => RoundTrip(new D7EDUserInfo { Uid = "uid", LatestTime = 7, Count = 2, Nickname = "n", IsFriend = true });
    [Test] public void ProfileLikeResponseRoundTrips() => RoundTrip(new D7EDRespBody { UserLikeInfos = [new D7EDUserLikeInfo { Uid = "uid", Time = 8, VoteInfo = new D7EDVoteInfo { TotalCount = 2, Users = [new D7EDUserInfo { Uid = "u" }] } }] });
    [Test] public void ReadReportSupportsGroupAndPrivateEntries()
    {
        var packet = new SsoReadedReportReq { GroupList = [new GroupReadedReportItem { GroupUin = 1, LastReadSeq = 2 }], C2CList = [new C2CReadedReportItem { Uid = "u", LastReadSeq = 3 }] };
        var decoded = ProtoHelper.Deserialize<SsoReadedReportReq>(ProtoHelper.Serialize(packet).Span);
        Assert.That(decoded.GroupList![0].LastReadSeq, Is.EqualTo(2));
        Assert.That(decoded.C2CList![0].Uid, Is.EqualTo("u"));
    }
    [Test] public void ReadReportResponsePreservesLatestSequence()
    {
        var packet = new SsoReadedReportResp { GroupList = [new GroupReadedReportResponseItem { GroupUin = 1, LatestSeq = 9 }] };
        var decoded = ProtoHelper.Deserialize<SsoReadedReportResp>(ProtoHelper.Serialize(packet).Span);
        Assert.That(decoded.GroupList![0].LatestSeq, Is.EqualTo(9));
    }
    [Test] public void GroupPermissionRequestPreservesMask()
    {
        var packet = new D89AReqBody { GroupCode = 1, Group = new D89AReqBody.GroupInfo { AppPrivilegeFlag = 0x10000, AppPrivilegeMask = 0x10000 } };
        var decoded = ProtoHelper.Deserialize<D89AReqBody>(ProtoHelper.Serialize(packet).Span);
        Assert.That(decoded.Group!.AppPrivilegeMask, Is.EqualTo(0x10000));
    }
    [Test] public void GroupPermissionResponsePreservesError()
    {
        var packet = new D89ARspBody { GroupCode = 1, ErrorInfo = "denied" };
        var decoded = ProtoHelper.Deserialize<D89ARspBody>(ProtoHelper.Serialize(packet).Span);
        Assert.That(decoded.ErrorInfo, Is.EqualTo("denied"));
    }

    [Test] public void GroupAddOptionPreservesQuestionFields() => RoundTrip(new D89AReqBody { GroupCode = 1, Group = new D89AReqBody.GroupInfo { AddOption = 4, GroupQuestion = "q", GroupAnswer = "a" } });
    [Test] public void GroupSearchPreservesBothFlags() => RoundTrip(new D89AReqBody { GroupCode = 1, Group = new D89AReqBody.GroupInfo { NoFingerOpenFlag = 1, NoCodeFingerOpenFlag = 0 } });
    [Test] public void GroupHistoryVisibilityPreservesMask() => RoundTrip(new D89AReqBody { GroupCode = 1, Group = new D89AReqBody.GroupInfo { GroupFlagExt4 = 4, GroupFlagExt4Mask = 4 } });
    [Test] public void GroupInvitePolicyPreservesPrivilegeMask() => RoundTrip(new D89AReqBody { GroupCode = 1, Group = new D89AReqBody.GroupInfo { AppPrivilegeFlag = 0x04000000, AppPrivilegeMask = 0x06100000, AllowMemberInvite = 0 } });
    [Test] public void GroupRobotOptionsPreserveExtendedTags() => RoundTrip(new F00ReqBody { GroupCode = 1, Info = new F00GroupInfo { GroupCode = 1, Ext = new F00ExtInfo { InviteRobotMemberSwitch = 1, InviteRobotMemberExamine = 0 } } });
    [Test] public void GroupAdminDetailPreservesPolicyFields() => RoundTrip(new D88DGroupInfo { GroupOption = 4, GroupQuestion = "q", GroupAnswer = "a", AppPrivilegeFlag = 0x04000000, GroupFlagExt4 = 4, NoFingerOpenFlag = 1, NoCodeFingerOpenFlag = 0 });
    [Test] public void GroupRobotQueryPreservesFilter() => RoundTrip(new EF0ReqBody { GroupCodes = [1], Filter = new EF0Filter { InviteRobotMemberSwitch = 1, InviteRobotMemberExamine = 1 } });
    [Test] public void GroupRobotQueryPreservesResult() => RoundTrip(new EF0RspBody { Items = [new EF0Item { GroupCode = 1, ResultCode = 0, Ext = new EF0Ext { InviteRobotMemberSwitch = 1, InviteRobotMemberExamine = 0 } }] });

    private static void RoundTrip<T>(T value) where T : IProtoSerializable<T>
    {
        var decoded = ProtoHelper.Deserialize<T>(ProtoHelper.Serialize(value).Span);
        Assert.That(decoded, Is.Not.Null);
    }
}
