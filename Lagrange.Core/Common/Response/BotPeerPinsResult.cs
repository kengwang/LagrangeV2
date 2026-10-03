namespace Lagrange.Core.Common.Response;
public sealed class BotPeerPinsResult
{
    public IReadOnlyList<long> PeerUins { get; init; } = [];
}
