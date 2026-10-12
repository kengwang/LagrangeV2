using Lagrange.Core.Common.Entity;

namespace Lagrange.Core.Events.EventArgs;

/// <summary>A received SSO packet, after decryption and decompression, before business parsing.</summary>
/// <param name="packet">The parsed SSO packet whose fields are captured.</param>
public sealed class BotPacketReceivedEvent(BotSsoPacket packet) : EventBase
{
    /// <summary>The SSO command.</summary>
    public string Command { get; } = packet.Command;
    /// <summary>The decrypted and decompressed payload, with no business decoding applied.</summary>
    public ReadOnlyMemory<byte> Data { get; } = packet.Data;
    /// <summary>The received SSO sequence number.</summary>
    public int Sequence { get; } = packet.Sequence;
    /// <summary>The QQ return code.</summary>
    public int RetCode { get; } = packet.RetCode;
    /// <summary>The additional response information supplied by QQ.</summary>
    public string Extra { get; } = packet.Extra;

    /// <inheritdoc />
    public override string ToEventMessage() => $"{nameof(BotPacketReceivedEvent)}: {Command} [{Sequence}] ({RetCode}), {Data.Length} bytes";
}
