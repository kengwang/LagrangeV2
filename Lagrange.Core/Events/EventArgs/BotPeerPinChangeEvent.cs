namespace Lagrange.Core.Events.EventArgs;
public sealed class BotPeerPinChangeEvent(string scene, long peerId, bool isPinned) : EventBase
{
    public string Scene { get; } = scene;
    public long PeerId { get; } = peerId;
    public bool IsPinned { get; } = isPinned;
    public override string ToEventMessage() => $"{nameof(BotPeerPinChangeEvent)}: {Scene} {PeerId} {(IsPinned ? "pinned" : "unpinned")}";
}
