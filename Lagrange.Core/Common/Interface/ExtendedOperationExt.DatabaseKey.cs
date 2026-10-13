using Lagrange.Core.Common.Response;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service.Migration;

namespace Lagrange.Core.Common.Interface;

public static partial class ExtendedOperationExt
{
    /// <summary>Requests the current account's database password, with or without existing key metadata.</summary>
    /// <param name="context">The authenticated bot session.</param>
    /// <param name="mode">Encrypt creates key metadata; decrypt exchanges existing metadata for a key.</param>
    /// <param name="keyMeta">Exactly 128 hexadecimal characters for decrypt; absent for encrypt.</param>
    /// <param name="ct">Cancels the request.</param>
    /// <returns>The password and returned metadata; metadata may be empty in decrypt mode.</returns>
    public static async Task<BotDatabaseKeyResult> RequestDecryptKey(this BotContext context, DatabaseKeyMode mode,
        string? keyMeta = null, CancellationToken ct = default)
    {
        switch (mode)
        {
            case DatabaseKeyMode.Encrypt:
                if (keyMeta is not null) throw new ArgumentException("key_meta is not accepted in encrypt mode.", nameof(keyMeta));
                return (await context.EventContext.SendEvent<RequestEncryptKeyEventResp>(new RequestEncryptKeyEventReq(), ct)).Result;
            case DatabaseKeyMode.Decrypt:
                if (keyMeta is null || keyMeta.Length != 128 || keyMeta.Any(x => !Uri.IsHexDigit(x)))
                    throw new ArgumentException("key_meta must contain 128 hexadecimal characters.", nameof(keyMeta));
                var response = (await context.EventContext.SendEvent<RequestDatabaseKeyEventResp>(
                    new RequestDatabaseKeyEventReq(new Oidb0xcdeReq { Info = new() { Db_salt = keyMeta } }), ct)).Body;
                if (string.IsNullOrEmpty(response.Info?.DbKey)) throw new OperationException(-500, "Database key was not returned.");
                return new BotDatabaseKeyResult(response.Info.DbKey, response.Info.KeyMeta);
            default:
                throw new ArgumentOutOfRangeException(nameof(mode));
        }
    }
}
