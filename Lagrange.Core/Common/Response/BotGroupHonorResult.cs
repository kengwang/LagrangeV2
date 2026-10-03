namespace Lagrange.Core.Common.Response;

public sealed class BotGroupHonorResult
{
    public required string HonorType { get; init; }
    public required IReadOnlyList<BotGroupHonorItem> Items { get; init; }
}

public sealed class BotGroupHonorItem
{
    public long? UserId { get; init; }
    public string Nickname { get; init; } = string.Empty;
    public string Avatar { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}
