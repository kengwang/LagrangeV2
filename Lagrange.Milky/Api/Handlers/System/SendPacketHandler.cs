using FastEndpoints;
using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Models;

namespace Lagrange.Milky.Api.Handlers.System;

/// <summary>Lagrange extension for sending arbitrary SSO payloads.</summary>
/// <param name="lagrange">The bot whose authenticated connection is used.</param>
public sealed class SendPacketHandler(BotContext lagrange) : Endpoint<SendPacketHandler.Request, MilkyApiResponse<RawPacketData>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/send_packet");
    }

    /// <inheritdoc />
    public override async Task<MilkyApiResponse<RawPacketData>> ExecuteAsync(Request request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Command) || request.Data is null || request.WaitForResponse is null)
            return new(-400, "cmd must be nonblank; data and rsp must not be null.");

        byte[] data;
        try { data = Convert.FromHexString(request.Data); }
        catch (FormatException) { return new(-400, "data must be a continuous hexadecimal string with an even length."); }

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        using var lifetime = CancellationTokenSource.CreateLinkedTokenSource(ct, timeout.Token);
        try
        {
            var response = await lagrange.SendRawPacket(new BotSsoPacket(request.Command, data), request.WaitForResponse.Value, lifetime.Token);
            return response is null ? new() : new(RawPacketData.FromPacket(response));
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (OperationCanceledException) when (timeout.IsCancellationRequested) { return new(-408, "Raw packet operation timed out after 10 seconds."); }
        catch (Exception ex) { return new(-500, ex.Message); }
    }

    /// <summary>Raw SSO request parameters. Omitted data is empty; omitted rsp is true.</summary>
    public sealed class Request
    {
        /// <summary>The required nonblank SSO command.</summary>
        [JsonPropertyName("cmd")] public string? Command { get; set; }
        /// <summary>The payload as continuous hexadecimal; null is invalid.</summary>
        [JsonPropertyName("data")] public string? Data { get; set; } = string.Empty;
        /// <summary>Whether to wait for the raw response; null is invalid.</summary>
        [JsonPropertyName("rsp")] public bool? WaitForResponse { get; set; } = true;
    }
}
