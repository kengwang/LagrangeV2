namespace Lagrange.Core.Events.EventArgs;
public sealed class BotGroupCardChangeEvent(long groupUin, long userUin, string oldCard, string newCard) : EventBase
{
    public long GroupUin { get; } = groupUin;
    public long UserUin { get; } = userUin;
    public string OldCard { get; } = oldCard;
    public string NewCard { get; } = newCard;
    public override string ToEventMessage() => $"Group {GroupUin}, member {UserUin}: {OldCard} -> {NewCard}";
}
