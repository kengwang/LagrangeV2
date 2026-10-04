using Lagrange.Core.Common.Interface;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Utility;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Services.Message;
using Lagrange.Core.Services;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Message;
namespace Lagrange.Core.Test.Protocol;

public sealed class HistorySyncProtocolTest
{
    private static BotFriend Friend(long uin) => new(uin, "test", $"u{uin}", "", "", "", new(0, "", 0, 0));
    private static BotMessage PrivateMessage(ulong sequence, long sender = 2, long receiver = 1) => new(Friend(sender), Friend(receiver), 0) { ClientSequence = sequence, Sequence = 9999 };
    private static BotMessage GroupMessage(ulong sequence, long groupId = 3)
    {
        var group = new BotGroup(groupId, "test", 1, 100, 0, null, null, null);
        var member = new BotGroupMember(group, 2, "u2", "test", GroupMemberPermission.Member, 0, null, null, 0, 0, 0);
        return new(member, group, 0) { Sequence = sequence, ClientSequence = 9999 };
    }

    [Test]
    public void SequencePagesSortByAuthoritativeSequenceAndAllowBothPrivateDirections()
    {
        var privatePage = HistorySyncExt.CompleteSequencePage([PrivateMessage(12, 1, 2), PrivateMessage(10)], MessageType.Private, 2, 1, 10, 29, 100);
        Assert.That(privatePage.Messages.Select(x => x.ClientSequence), Is.EqualTo(new ulong[] { 10, 12 }));
        Assert.That(privatePage.NextSequence, Is.EqualTo(30));
        Assert.That(privatePage.IsComplete, Is.False);
        var groupPage = HistorySyncExt.CompleteSequencePage([GroupMessage(12), GroupMessage(10)], MessageType.Group, 3, 1, 10, 29, 29);
        Assert.That(groupPage.Messages.Select(x => x.Sequence), Is.EqualTo(new ulong[] { 10, 12 }));
        Assert.That(groupPage.NextSequence, Is.Null);
        Assert.That(groupPage.IsComplete, Is.True);
    }

    [Test]
    public void EmptyDeletedWindowStillAdvancesToFollowingWindow()
    {
        var page = HistorySyncExt.CompleteSequencePage([], MessageType.Group, 3, 1, 10, 29, 100);
        Assert.That(page.Messages, Is.Empty);
        Assert.That(page.NextSequence, Is.EqualTo(30));
        Assert.That(page.IsComplete, Is.False);
    }

    [Test]
    public void GroupHistoryAcceptsSelfMemberReceiverFromMessagePacker()
    {
        var original = GroupMessage(10);
        var group = ((BotGroupMember)original.Contact).Group;
        var self = new BotGroupMember(group, 1, "self", "bot", GroupMemberPermission.Member, 0, null, null, 0, 0, 0);
        var incoming = new BotMessage(original.Contact, self, 0) { Sequence = 10 };
        var page = HistorySyncExt.CompleteSequencePage([incoming], MessageType.Group, 3, 1, 10, 29, 100);
        Assert.That(page.Messages.Single(), Is.SameAs(incoming));
        var foreign = new BotGroup(4, "other", 1, 100, 0, null, null, null);
        var wrongReceiver = new BotGroupMember(foreign, 1, "self", "bot", GroupMemberPermission.Member, 0, null, null, 0, 0, 0);
        Assert.Throws<OperationException>(() => HistorySyncExt.CompleteSequencePage(
            [new BotMessage(original.Contact, wrongReceiver, 0) { Sequence = 10 }], MessageType.Group, 3, 1, 10, 29, 100));
        Assert.Throws<OperationException>(() => HistorySyncExt.CompleteSequencePage(
            [new BotMessage(GroupMessage(10, 4).Contact, self, 0) { Sequence = 10 }], MessageType.Group, 3, 1, 10, 29, 100));
    }

    [Test]
    public void SequencePageRejectsForeignIdentityOutOfRangeAndDuplicateMessages()
    {
        Assert.Throws<OperationException>(() => HistorySyncExt.CompleteSequencePage([PrivateMessage(9)], MessageType.Private, 2, 1, 10, 29, 100));
        Assert.Throws<OperationException>(() => HistorySyncExt.CompleteSequencePage([PrivateMessage(10, 4)], MessageType.Private, 2, 1, 10, 29, 100));
        Assert.Throws<OperationException>(() => HistorySyncExt.CompleteSequencePage([PrivateMessage(10, 2, 4)], MessageType.Private, 2, 1, 10, 29, 100));
        Assert.Throws<OperationException>(() => HistorySyncExt.CompleteSequencePage([GroupMessage(10, 4)], MessageType.Group, 3, 1, 10, 29, 100));
        Assert.Throws<OperationException>(() => HistorySyncExt.CompleteSequencePage([GroupMessage(30)], MessageType.Group, 3, 1, 10, 29, 100));
        Assert.Throws<OperationException>(() => HistorySyncExt.CompleteSequencePage([GroupMessage(10), GroupMessage(10)], MessageType.Group, 3, 1, 10, 29, 100));
    }
    [Test]
    public async Task SharedReadReportServiceBuildsProbeAndReturnsCompatibleResponse()
    {
        IService service = new MarkMessageReadService();
        var request = new HistorySyncProbeEventReq(HistorySyncExt.BuildProbe([123], []));
        Assert.That(Convert.ToHexString((await service.Build(request, null!)).Span), Is.EqualTo("0A02087B"));
        var response = await service.Parse(Convert.FromHexString("1A06187B205A2878"), null!);
        Assert.That(response, Is.InstanceOf<MarkMessageReadEventResp>());
        Assert.That(response, Is.TypeOf<MarkAllMessagesReadEventResp>());
        Assert.That(((MarkAllMessagesReadEventResp)response).Body.GroupList!.Single().LatestSeq, Is.EqualTo(120));
        var all = await service.Build(new MarkAllMessagesReadEventReq([123], []), null!);
        Assert.That(Convert.ToHexString(all.Span), Is.EqualTo("0A02087B"));
    }
    [Test]
    public void ProbeOmitsReadAcknowledgementFields()
    {
        var request = HistorySyncExt.BuildProbe([123, 123], [new(456, "u")]);
        Assert.That(Convert.ToHexString(ProtoHelper.Serialize(request).Span), Is.EqualTo("0A02087B1203120175"));
    }
    [Test]
    public void ProbeRejectsOversizedAndConflictingTargets()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => HistorySyncExt.BuildProbe(Enumerable.Range(1, 101).Select(x => (long)x).ToArray(), []));
        Assert.Throws<ArgumentException>(() => HistorySyncExt.BuildProbe([], [new(1, "u"), new(1, "v")]));
    }
    [Test]
    public void ProbeDecodesIndependentServerCursorFixture()
    {
        var response = ProtoHelper.Deserialize<SsoReadedReportResp>(Convert.FromHexString("1A06187B205A2878"));
        var state = HistorySyncExt.ParseProbe(response, [123], []);
        Assert.That(state.Groups.Single(), Is.EqualTo(new BotGroupHistoryState(123, 90, 120)));
    }
    [Test]
    public void ProbeRejectsMissingErrorAndInvertedResponses()
    {
        Assert.Throws<OperationException>(() => HistorySyncExt.ParseProbe(new(), [123], []));
        Assert.Throws<OperationException>(() => HistorySyncExt.ParseProbe(new() { GroupList = [new() { GroupUin = 123, ResultCode = 42 }] }, [123], []));
        Assert.Throws<OperationException>(() => HistorySyncExt.ParseProbe(new() { GroupList = [new() { GroupUin = 123, ReadSeq = 5, LatestSeq = 4 }] }, [123], []));
        Assert.Throws<OperationException>(() => HistorySyncExt.ParseProbe(new() { C2CList = [new() { Uid = "u", TargetUin = 999 }] }, [], [new(1, "u")]));
    }
    [Test]
    public void ForwardPageBoundsAreInclusiveAndOverflowSafe()
    {
        Assert.That(HistorySyncExt.PageEnd(101, 140, 20), Is.EqualTo(120));
        Assert.That(HistorySyncExt.PageEnd(121, 125, 20), Is.EqualTo(125));
        Assert.That(HistorySyncExt.PageEnd(126, 125, 20), Is.Null);
        Assert.That(HistorySyncExt.PageEnd(ulong.MaxValue - 1, ulong.MaxValue, 20), Is.EqualTo(ulong.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => HistorySyncExt.PageEnd(1, 100, 21));
    }
    [Test]
    public void RoamCursorPreservesRandomAndRejectsRepeats()
    {
        var input = new BotHistoryRoamCursor(100, 1);
        Assert.That(HistorySyncExt.RoamContinuation(input, 100, 2, false), Is.EqualTo(new BotHistoryRoamCursor(100, 2)));
        Assert.That(HistorySyncExt.RoamContinuation(input, 0, 0, true), Is.Null);
        Assert.Throws<OperationException>(() => HistorySyncExt.RoamContinuation(input, 100, 1, false));
        Assert.Throws<OperationException>(() => HistorySyncExt.RoamContinuation(input, 101, 1, false));
        var response = ProtoHelper.Deserialize<SsoGetRoamMsgRsp>(Convert.FromHexString("200128633007"));
        Assert.That(response.IsComplete, Is.True);
        Assert.That(response.Timestamp, Is.EqualTo(99));
        Assert.That(response.Random, Is.EqualTo(7));
    }

    [Test]
    public async Task RoamRequestUsesBackwardDirectionAndBothCursorFields()
    {
        IService service = new GetRoamMessageService();
        var wire = await service.Build(new GetRoamMessageEventReq("u", 100, 20, 7, 1), null!);
        Assert.That(Convert.ToHexString(wire.Span), Is.EqualTo("0A01751064180720142801"));
    }
}
