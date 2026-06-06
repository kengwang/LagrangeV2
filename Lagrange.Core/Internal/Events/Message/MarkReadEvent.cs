namespace Lagrange.Core.Internal.Events.Message;

internal class MarkReadEventReq(long groupUin, string? targetUid, ulong startSequence, uint time) : ProtocolEvent
{
    public long GroupUin { get; } = groupUin;

    public string? TargetUid { get; } = targetUid;

    public ulong StartSequence { get; } = startSequence;

    public uint Time { get; } = time;
}

internal class MarkReadEventResp : ProtocolEvent
{
    public static readonly MarkReadEventResp Default = new();
}
