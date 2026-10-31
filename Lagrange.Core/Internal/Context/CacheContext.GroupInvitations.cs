using Lagrange.Core.Common.Entity;

namespace Lagrange.Core.Internal.Context;

internal partial class CacheContext
{
    private readonly object _invitationGate = new();
    private readonly HashSet<(long Group, ulong Sequence, string Inviter, string Target)> _invitations = [];
    private readonly Queue<(long Group, ulong Sequence, string Inviter, string Target)> _invitationOrder = [];

    internal bool ObserveInvitation(BotGroupInviteNotification invitation)
    {
        var key = (invitation.GroupUin, invitation.Sequence, invitation.InviterUid, invitation.TargetUid);
        lock (_invitationGate)
        {
            if (!_invitations.Add(key)) return false;
            _invitationOrder.Enqueue(key);
            if (_invitationOrder.Count > 1024) _invitations.Remove(_invitationOrder.Dequeue());
            return true;
        }
    }
}
