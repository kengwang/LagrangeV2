using Lagrange.Core.Common.Entity;
using Lagrange.Core.Services;

namespace Lagrange.Core.Events.EventArgs;

/// <summary>A packet Trace diagnostic with fields preserved for logging adapters.</summary>
public sealed class BotPacketLogEvent : BotLogEvent
{
    private BotPacketLogEvent(BotSsoPacket packet, string data, ServiceAttribute? options)
        : base("PacketContext", LogLevel.Trace, options is null
            ? $"[↓] <{packet.Sequence}>[{packet.Command}]({packet.RetCode}) {data}"
            : $"[↑] <{packet.Sequence}>[{packet.Command}]({packet.RetCode}, {options.RequestType}|{options.EncryptType}) {data}", null)
    {
        Direction = options is null ? "Incoming" : "Outgoing";
        Sequence = packet.Sequence;
        Command = packet.Command;
        RetCode = packet.RetCode;
        Data = data;
        DataLength = packet.Data.Length;
        Extra = packet.Extra;
        RequestType = options?.RequestType.ToString();
        EncryptType = options?.EncryptType.ToString();
    }

    public string Direction { get; }
    public int Sequence { get; }
    public string Command { get; }
    public int RetCode { get; }
    /// <summary>The decrypted SSO payload as uppercase hexadecimal.</summary>
    public string Data { get; }
    public int DataLength { get; }
    public string Extra { get; }
    public string? RequestType { get; }
    public string? EncryptType { get; }

    internal static BotPacketLogEvent Create(BotSsoPacket packet, ServiceAttribute? options = null)
        => new(packet, Convert.ToHexString(packet.Data.Span), options);
}
