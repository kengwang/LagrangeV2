namespace Lagrange.Core.Common.Response;

public sealed class BotQzoneMessageListResult
{
    public required IReadOnlyList<BotQzoneMessage> Messages { get; init; }
    public int Total { get; init; }
}

public sealed class BotQzoneMessage
{
    public required string Id { get; init; }
    public string Content { get; init; } = string.Empty;
    public long Time { get; init; }
    public int CommentCount { get; init; }
    public bool IsPrivate { get; init; }
    public required IReadOnlyList<string> Images { get; init; }
}

public sealed class BotQzoneCommentResult
{
    public string CommentId { get; init; } = string.Empty;
}

public sealed class BotQzonePublishResult
{
    public required string MessageId { get; init; }
    public long Time { get; init; }
}
