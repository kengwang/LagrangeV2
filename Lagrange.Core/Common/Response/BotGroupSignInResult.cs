namespace Lagrange.Core.Common.Response;

public sealed class BotGroupSignInResult
{
    public required IReadOnlyList<BotGroupSignInMember> Members { get; init; }
}

public sealed class BotGroupSignInMember
{
    public long UserId { get; init; }
    public string Nickname { get; init; } = string.Empty;
    public long Time { get; init; }
    public int Rank { get; init; }
}
