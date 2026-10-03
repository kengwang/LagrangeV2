namespace Lagrange.Core.Common.Response;

public sealed class BotFlashCreateResult
{
    public required string FilesetUuid { get; init; }
    public required string UploadKey { get; init; }
    public string? UploadUrl { get; init; }
    public ulong Expire { get; init; }
    public uint Ttl { get; init; }
}
