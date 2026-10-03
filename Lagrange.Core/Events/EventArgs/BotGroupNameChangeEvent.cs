namespace Lagrange.Core.Events.EventArgs;

public sealed class BotGroupNameChangeEvent(long groupUin, string name, long operatorUin) : EventBase
{
    public long GroupUin { get; } = groupUin;
    public string Name { get; } = name;
    public long OperatorUin { get; } = operatorUin;
    public override string ToEventMessage() => $"{nameof(BotGroupNameChangeEvent)}: {GroupUin} -> {Name}";
}
