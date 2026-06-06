namespace Lagrange.Core.Common.Entity;

public class BotPeerPins(IReadOnlyList<BotFriend> friends, IReadOnlyList<BotGroup> groups)
{
    public IReadOnlyList<BotFriend> Friends { get; } = friends;

    public IReadOnlyList<BotGroup> Groups { get; } = groups;
}
