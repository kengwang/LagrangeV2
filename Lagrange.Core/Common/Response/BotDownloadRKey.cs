namespace Lagrange.Core.Common.Response;
/// <summary>A server-issued download credential and its scope and lifetime.</summary>
public sealed record BotDownloadRKey(uint? Type, string Key, uint? CreatedAt, ulong TtlSeconds);
