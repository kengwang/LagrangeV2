namespace Lagrange.Core.Common.Response;

public sealed class BotCustomFaceListResult
{
    public required IReadOnlyList<string> FaceIds { get; init; }
    public uint TotalCount { get; init; }
}

public sealed class BotCustomFaceDetailResult
{
    public required IReadOnlyList<BotCustomFaceDetail> Entries { get; init; }
}

public sealed class BotCustomFaceDetail
{
    public required string FaceId { get; init; }
    public required string Description { get; init; }
}
