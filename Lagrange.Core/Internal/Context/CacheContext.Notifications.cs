using Lagrange.Core.Events.EventArgs;
namespace Lagrange.Core.Internal.Context;

internal partial class CacheContext
{
    private BotOnlineDevice[]? _onlineDevices;
    private readonly object _notificationLock = new();
    private readonly Dictionary<(long Group, long User), string> _observedCards = [];
    internal IReadOnlyList<BotOnlineDevice>? OnlineDevices => Volatile.Read(ref _onlineDevices)?.ToArray();
    internal void SetOnlineDevices(IEnumerable<BotOnlineDevice> devices) => Volatile.Write(ref _onlineDevices, devices.ToArray());
    internal void ResetSessionNotifications()
    {
        Volatile.Write(ref _onlineDevices, null);
        lock (_notificationLock) _observedCards.Clear();
    }
    internal async Task UpdateRemark(long uin, string uid, string remark)
    {
        var friend = _friends?.FirstOrDefault(x => (uin > 0 && x.Uin == uin) || (!string.IsNullOrWhiteSpace(uid) && x.Uid == uid));
        if (friend is not null) friend.Remarks = remark;
        await _strangersLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (uin > 0 && _strangersWithUin.TryGetValue(uin, out var stranger)) stranger.Remark = remark;
            if (!string.IsNullOrWhiteSpace(uid) && _strangersWithUid.TryGetValue(uid, out stranger)) stranger.Remark = remark;
        }
        finally { _strangersLock.Release(); }
    }
    internal BotGroupCardChangeEvent? ObserveCard(long group, long user, string card)
    {
        lock (_notificationLock)
        {
            bool known = _observedCards.TryGetValue((group, user), out var previous);
            _observedCards[(group, user)] = card;
            if (_members.TryGetValue(group, out var members))
            {
                var member = members.FirstOrDefault(x => x.Uin == user);
                if (member is not null) member.MemberCard = card;
            }
            return known && previous != card ? new(group, user, previous!, card) : null;
        }
    }
}
