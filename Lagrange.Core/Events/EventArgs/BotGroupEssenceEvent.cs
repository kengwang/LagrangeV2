namespace Lagrange.Core.Events.EventArgs;

/// <summary>Raised when a group message is added to or removed from the essence list.</summary>
public sealed class BotGroupEssenceEvent(
    long groupUin,
    ulong sequence,
    uint random,
    long senderUin,
    long operatorUin,
    bool isSet,
    uint timestamp) : EventBase
{
    public long GroupUin { get; } = groupUin;
    public ulong Sequence { get; } = sequence;
    public uint Random { get; } = random;
    public long SenderUin { get; } = senderUin;
    public long OperatorUin { get; } = operatorUin;
    public bool IsSet { get; } = isSet;
    public uint Timestamp { get; } = timestamp;

    public override string ToEventMessage() =>
        $"{nameof(BotGroupEssenceEvent)}: {GroupUin}-{Sequence} {(IsSet ? "set" : "removed")} by {OperatorUin}";
}
