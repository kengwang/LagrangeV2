namespace Lagrange.Core.Common.Entity;

/// <summary>Preserves server group-notification records whose wire type is newer than this client.</summary>
public sealed class BotGroupUnknownNotification(long groupUin, ulong sequence, ulong rawType, long targetUin, string targetUid, string comment)
    : BotGroupNotificationBase(groupUin, sequence, BotGroupNotificationType.Unknown, targetUin, targetUid)
{
    public ulong RawType { get; } = rawType;
    public string Comment { get; } = comment;
}
