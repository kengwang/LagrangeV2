namespace Lagrange.Core.Common.Entity;

public sealed class BotDoubtFriendRequest
{
    public required string Uid { get; init; }
    public long UserId { get; init; }
    public string Nick { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string GroupCode { get; init; } = string.Empty;
    public long RequestTime { get; init; }
}
