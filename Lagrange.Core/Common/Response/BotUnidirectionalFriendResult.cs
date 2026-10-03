namespace Lagrange.Core.Common.Response;

public sealed class BotUnidirectionalFriendResult
{
    public required IReadOnlyList<IReadOnlyDictionary<string, string>> Entries { get; init; }
}
