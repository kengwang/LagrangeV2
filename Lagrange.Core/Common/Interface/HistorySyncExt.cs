using Lagrange.Core.Common.Response;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Logic;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Message;
namespace Lagrange.Core.Common.Interface;

/// <summary>Bounded history synchronization without read acknowledgements or local message storage.</summary>
public static class HistorySyncExt
{
    /// <summary>Probes at most 100 targets of each kind without changing their read positions.</summary>
    public static async Task<BotHistorySyncState> ProbeHistorySyncState(this BotContext context, IReadOnlyList<long> groupUins, IReadOnlyList<BotHistoryPrivateTarget> privateTargets, CancellationToken ct = default)
    {
        var request = BuildProbe(groupUins, privateTargets);
        ct.ThrowIfCancellationRequested();
        if (request.GroupList!.Count == 0 && request.C2CList!.Count == 0) return new([], []);
        var response = await context.EventContext.SendEvent<MarkAllMessagesReadEventResp>(new HistorySyncProbeEventReq(request), ct);
        return ParseProbe(response.Body, groupUins.Distinct().ToArray(), privateTargets.Distinct().ToArray());
    }

    internal static SsoReadedReportReq BuildProbe(IReadOnlyList<long> groups, IReadOnlyList<BotHistoryPrivateTarget> users)
    {
        ArgumentNullException.ThrowIfNull(groups); ArgumentNullException.ThrowIfNull(users);
        var uniqueGroups = groups.Distinct().ToArray();
        var uniqueUsers = users.Distinct().ToArray();
        if (uniqueGroups.Length > 100 || uniqueUsers.Length > 100) throw new ArgumentOutOfRangeException(nameof(groups), "At most 100 targets of each kind may be probed.");
        foreach (long group in uniqueGroups) ArgumentOutOfRangeException.ThrowIfNegativeOrZero(group);
        foreach (var user in uniqueUsers) { ArgumentNullException.ThrowIfNull(user); ArgumentOutOfRangeException.ThrowIfNegativeOrZero(user.UserUin); ArgumentException.ThrowIfNullOrWhiteSpace(user.Uid); }
        if (uniqueUsers.Select(x => x.UserUin).Distinct().Count() != uniqueUsers.Length || uniqueUsers.Select(x => x.Uid).Distinct().Count() != uniqueUsers.Length)
            throw new ArgumentException("Private target identities conflict.", nameof(users));
        // Null read sequence/time fields are omitted; explicit zero would still serialize.
        return new() { GroupList = uniqueGroups.Select(x => new GroupReadedReportItem { GroupUin = (ulong)x }).ToList(), C2CList = uniqueUsers.Select(x => new C2CReadedReportItem { Uid = x.Uid }).ToList() };
    }

    internal static BotHistorySyncState ParseProbe(SsoReadedReportResp response, IReadOnlyList<long> groups, IReadOnlyList<BotHistoryPrivateTarget> users)
    {
        if (response.ResultCode != 0) throw new OperationException((int)response.ResultCode, response.ErrorMessage);
        var groupResults = new List<BotGroupHistoryState>();
        foreach (long group in groups)
        {
            var matches = response.GroupList?.Where(x => x.GroupUin == (ulong)group).ToArray() ?? [];
            if (matches.Length != 1) throw new OperationException(-1, $"Missing or ambiguous history state for group {group}.");
            var item = matches[0];
            if (item.ResultCode != 0) throw new OperationException((int)item.ResultCode, item.ErrorMessage);
            if (item.LatestSeq < item.ReadSeq) throw new OperationException(-1, "Latest history sequence precedes read sequence.");
            groupResults.Add(new(group, item.ReadSeq, item.LatestSeq));
        }
        var privateResults = new List<BotPrivateHistoryState>();
        foreach (var user in users)
        {
            var matches = response.C2CList?.Where(x => x.Uid == user.Uid || x.TargetUin == (ulong)user.UserUin).ToArray() ?? [];
            if (matches.Length != 1) throw new OperationException(-1, $"Missing or ambiguous history state for user {user.UserUin}.");
            var item = matches[0];
            if (item.ResultCode != 0) throw new OperationException((int)item.ResultCode, item.ErrorMessage);
            if ((item.TargetUin != 0 && item.TargetUin != (ulong)user.UserUin) || (!string.IsNullOrEmpty(item.Uid) && item.Uid != user.Uid)) throw new OperationException(-1, "Private history response identity mismatch.");
            if (item.LatestSeq < item.ReadSeq) throw new OperationException(-1, "Latest history sequence precedes read sequence.");
            privateResults.Add(new(user.UserUin, user.Uid, item.ReadSeq, item.LatestSeq, item.LastMsgTime));
        }
        return new(groupResults, privateResults);
    }

    /// <summary>Fetches at most 20 forward group sequence slots up to a fixed synchronization horizon.</summary>
    public static async Task<BotHistorySyncPage> GetGroupHistorySyncPage(this BotContext context, long groupUin, ulong nextSequence, ulong latestSequence, uint count = 20, CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(groupUin);
        var end = PageEnd(nextSequence, latestSequence, count);
        ct.ThrowIfCancellationRequested();
        if (end is null) return new([], null, true);
        var messages = await context.GetGroupMessage(groupUin, nextSequence, end.Value, ct);
        return CompleteSequencePage(messages, MessageType.Group, groupUin, context.BotUin, nextSequence, end.Value, latestSequence);
    }

    /// <summary>Fetches at most 20 forward private sequence slots up to a fixed synchronization horizon.</summary>
    public static async Task<BotHistorySyncPage> GetPrivateHistorySyncPage(this BotContext context, long userUin, ulong nextSequence, ulong latestSequence, uint count = 20, CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userUin);
        var end = PageEnd(nextSequence, latestSequence, count);
        ct.ThrowIfCancellationRequested();
        if (end is null) return new([], null, true);
        var messages = await context.GetC2CMessage(userUin, nextSequence, end.Value, ct);
        return CompleteSequencePage(messages, MessageType.Private, userUin, context.BotUin, nextSequence, end.Value, latestSequence);
    }

    internal static BotHistorySyncPage CompleteSequencePage(IReadOnlyList<BotMessage> messages, MessageType type, long peerUin, long selfUin, ulong start, ulong end, ulong latest)
    {
        var seen = new HashSet<ulong>();
        foreach (var message in messages)
        {
            bool correctPeer = type == MessageType.Group
                ? message.Contact is BotGroupMember member && member.Group.GroupUin == peerUin
                    && (message.Receiver is BotGroup group && group.GroupUin == peerUin
                        || message.Receiver is BotGroupMember receiver && receiver.Group.GroupUin == peerUin)
                : (message.Contact.Uin == peerUin && message.Receiver.Uin == selfUin) || (message.Contact.Uin == selfUin && message.Receiver.Uin == peerUin);
            if (message.Type != type || !correctPeer) throw new OperationException(-1, "History page contains a message from another conversation.");
            ulong sequence = type == MessageType.Group ? message.Sequence : message.ClientSequence;
            if (sequence < start || sequence > end) throw new OperationException(-1, "History page contains a sequence outside its requested window.");
            if (!seen.Add(sequence)) throw new OperationException(-1, "History page contains duplicate message sequences.");
        }
        var ordered = messages.OrderBy(x => type == MessageType.Group ? x.Sequence : x.ClientSequence).ToArray();
        // Advance by the requested window even when all its messages were deleted.
        return new(ordered, end == latest ? null : end + 1, end == latest);
    }

    internal static ulong? PageEnd(ulong next, ulong latest, uint count)
    {
        ArgumentOutOfRangeException.ThrowIfZero(next);
        if (count is 0 or > 20) throw new ArgumentOutOfRangeException(nameof(count), "A page contains 1 to 20 sequence slots.");
        return next > latest ? null : next + Math.Min(latest - next, count - 1);
    }

    /// <summary>Fetches one private roam page, preserving both server cursor fields for continuation.</summary>
    public static async Task<BotHistoryRoamPage> GetPrivateHistoryRoamPage(this BotContext context, long userUin, BotHistoryRoamCursor cursor, uint count = 20, CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userUin); ArgumentNullException.ThrowIfNull(cursor);
        if (count is 0 or > 20) throw new ArgumentOutOfRangeException(nameof(count));
        ArgumentOutOfRangeException.ThrowIfZero(cursor.Time);
        var uid = context.CacheContext.ResolveCachedUid(userUin) ?? throw new InvalidTargetException(userUin);
        var response = await context.EventContext.SendEvent<GetRoamMessageEventResp>(new GetRoamMessageEventReq(uid, cursor.Time, count, cursor.Random, 1), ct);
        if (!string.IsNullOrEmpty(response.PeerUid) && response.PeerUid != uid) throw new OperationException(-1, "Private history response identity mismatch.");
        bool complete = response.IsComplete || (response.Time == 0 && response.Random == 0);
        var continuation = RoamContinuation(cursor, response.Time, response.Random, complete);
        var messages = new List<BotMessage>();
        var logic = context.EventContext.GetLogic<MessagingLogic>();
        foreach (var chain in response.Chains) { ct.ThrowIfCancellationRequested(); messages.Add(await logic.Parse(chain)); }
        return new(messages, continuation, complete);
    }

    internal static BotHistoryRoamCursor? RoamContinuation(BotHistoryRoamCursor input, uint time, uint random, bool complete)
    {
        if (complete) return null;
        if (time == 0 || time > input.Time || (time == input.Time && random == input.Random)) throw new OperationException(-1, "History server returned a missing or non-advancing cursor.");
        return new(time, random);
    }
}
