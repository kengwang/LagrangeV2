namespace Lagrange.Core.Common.Response;

public sealed class BotGroupEssenceResult
{
    public required IReadOnlyList<BotGroupEssenceMessage> Messages { get; init; }
    public bool IsEnd { get; init; }
    public int GroupRole { get; init; }
}

public sealed class BotGroupEssenceMessage
{
    public required string GroupCode { get; init; }
    public ulong MessageSequence { get; init; }
    public uint MessageRandom { get; init; }
    public required string SenderUin { get; init; }
    public string SenderNick { get; init; } = string.Empty;
    public long SenderTime { get; init; }
    public string AddDigestUin { get; init; } = string.Empty;
    public string AddDigestNick { get; init; } = string.Empty;
    public long AddDigestTime { get; init; }
    public bool CanBeRemoved { get; init; }
    public string? Text { get; init; }
}
