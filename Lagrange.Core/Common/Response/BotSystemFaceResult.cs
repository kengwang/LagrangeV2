namespace Lagrange.Core.Common.Response;

public sealed class BotSystemFacePack
{
    public required string PackName { get; init; }
    public required IReadOnlyList<BotSystemFace> Faces { get; init; }
}

public sealed class BotSystemFace
{
    public required string Sid { get; init; }
    public required string Description { get; init; }
    public required string EmCode { get; init; }
    public int? CategoryId { get; init; }
    public string? Url { get; init; }
    public IReadOnlyList<string> Aliases { get; init; } = [];
}
