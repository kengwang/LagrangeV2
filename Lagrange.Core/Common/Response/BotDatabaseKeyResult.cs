namespace Lagrange.Core.Common.Response;

/// <summary>The QQ database key operation to perform.</summary>
public enum DatabaseKeyMode
{
    Encrypt,
    Decrypt
}

/// <summary>A database password and the server-provided database key metadata.</summary>
public sealed record BotDatabaseKeyResult(string Key, string KeyMeta);
