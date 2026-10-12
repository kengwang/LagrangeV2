using System.Text.Json.Serialization;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Events.EventArgs;

namespace Lagrange.Milky.Models;

/// <summary>Wire representation of a decrypted, decompressed SSO packet.</summary>
public sealed class RawPacketData
{
    /// <summary>The SSO command.</summary>
    [JsonPropertyName("cmd")] public required string Command { get; init; }
    /// <summary>The payload as continuous uppercase hexadecimal.</summary>
    [JsonPropertyName("data")] public required string Data { get; init; }
    /// <summary>The SSO sequence number.</summary>
    [JsonPropertyName("sequence")] public int Sequence { get; init; }
    /// <summary>The QQ return code, including nonzero protocol errors.</summary>
    [JsonPropertyName("ret_code")] public int RetCode { get; init; }
    /// <summary>The additional response information supplied by QQ.</summary>
    [JsonPropertyName("extra")] public required string Extra { get; init; }

    internal static RawPacketData FromPacket(BotSsoPacket packet) => new()
    {
        Command = packet.Command, Data = Convert.ToHexString(packet.Data.Span),
        Sequence = packet.Sequence, RetCode = packet.RetCode, Extra = packet.Extra
    };

    internal static RawPacketData FromEvent(BotPacketReceivedEvent packet) => new()
    {
        Command = packet.Command, Data = Convert.ToHexString(packet.Data.Span),
        Sequence = packet.Sequence, RetCode = packet.RetCode, Extra = packet.Extra
    };
}
