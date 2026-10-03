namespace Lagrange.Core.Common.Response;

public sealed class BotGroupFileTransferResult
{
    public int SaveBusId { get; init; }
    public required string SaveFilePath { get; init; }
}
