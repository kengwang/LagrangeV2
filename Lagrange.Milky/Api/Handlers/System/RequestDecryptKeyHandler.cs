using FastEndpoints;
using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Exceptions;

namespace Lagrange.Milky.Api.Handlers.System;

/// <summary>Lagrange extension for the two QQ database key operations.</summary>
public sealed class RequestDecryptKeyHandler(BotContext lagrange)
    : Endpoint<RequestDecryptKeyHandler.Request, MilkyApiResponse<RequestDecryptKeyHandler.Result>>
{
    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/request_decrypt_key");
    }

    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        DatabaseKeyMode mode;
        switch (request.Mode)
        {
            case "encrypt":
                if (request.HasKeyMeta) return new(-400, "key_meta is not accepted in encrypt mode.");
                mode = DatabaseKeyMode.Encrypt;
                break;
            case "decrypt":
                if (request.KeyMeta is not { Length: 128 } || request.KeyMeta.Any(x => !Uri.IsHexDigit(x)))
                    return new(-400, "key_meta must contain 128 hexadecimal characters.");
                mode = DatabaseKeyMode.Decrypt;
                break;
            default:
                return new(-400, "mode must be encrypt or decrypt.");
        }

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        using var lifetime = CancellationTokenSource.CreateLinkedTokenSource(ct, timeout.Token);
        try
        {
            var result = await lagrange.RequestDecryptKey(mode, request.KeyMeta, lifetime.Token);
            return new(new Result { Key = result.Key, KeyMeta = result.KeyMeta });
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (OperationCanceledException) when (timeout.IsCancellationRequested) { return new(-408, "Database key operation timed out after 10 seconds."); }
        catch (OperationException ex) { return new(ex.Result, ex.ErrMsg ?? ex.Message); }
        catch (Exception ex) { return new(-500, ex.Message); }
    }

    public sealed class Request
    {
        // Presence matters: explicit null is also invalid in encrypt mode.
        private string? _keyMeta;
        [JsonPropertyName("mode")] public string? Mode { get; set; }
        [JsonPropertyName("key_meta")]
        public string? KeyMeta
        {
            get => _keyMeta;
            set { _keyMeta = value; HasKeyMeta = true; }
        }
        internal bool HasKeyMeta { get; private set; }
    }

    public sealed class Result
    {
        [JsonPropertyName("key")] public required string Key { get; init; }
        [JsonPropertyName("key_meta")] public required string KeyMeta { get; init; }
    }
}
