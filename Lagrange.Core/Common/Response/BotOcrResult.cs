namespace Lagrange.Core.Common.Response;

public sealed class BotOcrResult
{
    public required IReadOnlyList<BotOcrText> Texts { get; init; }
    public string Language { get; init; } = string.Empty;
}

public sealed class BotOcrText
{
    public required string Text { get; init; }
    public uint Confidence { get; init; }
    public IReadOnlyList<BotOcrCoordinate> Coordinates { get; init; } = [];
}

public readonly record struct BotOcrCoordinate(int X, int Y);
