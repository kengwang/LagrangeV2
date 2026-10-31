using Lagrange.Core.Common.Entity;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Message.Entities;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Message;

internal static class DatalineMessages
{
    internal static ReadOnlyMemory<byte> Build(BotMessage message, long selfUin, uint sourceTerminal)
    {
        var device = BotDeviceContact.Find(message.Receiver.Uin) ?? throw new ArgumentException("Unknown device contact.");
        if (message.Entities.Count == 0 || message.Entities.Any(x => x is not TextEntity)) throw new ArgumentException("Device chats require text-only messages.");
        var text = string.Concat(message.Entities.Cast<TextEntity>().Select(x => x.Text));
        if (text.Length == 0) throw new ArgumentException("Device message is empty.");
        var body = new DatalineBody
        {
            SubCommand = 4,
            Header = new DatalineHeader { SourceAppId = sourceTerminal == 1 ? 1u : 1001u, DestinationAppId = device.TerminalType == 1 ? 1u : 1001u,
                SourceUin = selfUin, DestinationUin = selfUin, SourceTerminal = sourceTerminal, DestinationTerminal = device.TerminalType },
            Generic = new DatalineGeneric { SessionId = checked((ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()), Buffer = ProtoHelper.Serialize(new DatalineText { Items = [new DatalineTextItem { Text = text }] }).ToArray() }
        };
        return ProtoHelper.Serialize(new PbSendMsgReq
        {
            RoutingHead = new SendRoutingHead { Trans0X211 = new Trans0X211 { ToUin = selfUin, Uid = device.Uid, CcCmd = 7 } },
            ContentHead = new SendContentHead { PkgNum = 1 },
            MessageBody = new MessageBody { MsgContent = ProtoHelper.Serialize(body) },
            ClientSequence = message.ClientSequence, Random = unchecked((uint)message.Random),
            Control = new SendMessageControl { MessageFlag = checked((int)message.Time) }
        });
    }

    internal static BotMessage Parse(CommonMessage packet, BotContext context)
    {
        var raw = packet.MessageBody?.MsgContent ?? throw new InvalidDataException("Device message body is missing.");
        var body = ProtoHelper.Deserialize<DatalineBody>(raw.Span);
        var header = body.Header ?? throw new InvalidDataException("Device message header is missing.");
        if (header.SourceUin != context.BotUin || header.DestinationUin != context.BotUin) throw new InvalidDataException("Device message belongs to another account.");
        var source = BotDeviceContact.SourceTerminal(context.Config.Protocol);
        var outgoing = header.SourceTerminal == source;
        var peerTerminal = outgoing ? header.DestinationTerminal : header.SourceTerminal;
        var device = BotDeviceContact.Devices.SingleOrDefault(x => x.TerminalType == peerTerminal) ?? throw new InvalidDataException("Unknown device terminal.");
        var self = new BotFriend(context.BotUin, context.BotInfo?.Name ?? string.Empty, context.Keystore.Uid, string.Empty, string.Empty, string.Empty, null!);
        var message = new BotMessage(outgoing ? self : device.AsFriend(), outgoing ? device.AsFriend() : self, packet.ContentHead.Time)
        { Sequence = packet.ContentHead.Sequence, ClientSequence = packet.ContentHead.ClientSequence, MessageId = packet.ContentHead.MsgUid, Random = packet.ContentHead.Random };
        if (body.Generic is { Type: 0 or 1 } generic)
        {
            var text = ProtoHelper.Deserialize<DatalineText>(generic.Buffer);
            var content = string.Concat(text.Items.Select(x => x.Text));
            if (content.Length > 0) message.Entities.Add(new TextEntity(content));
        }
        foreach (var file in body.Files)
        {
            if (string.IsNullOrWhiteSpace(file.FileName) && string.IsNullOrWhiteSpace(file.FileIndex) && file.SessionId == 0) continue;
            message.Entities.Add(new GroupFileEntity { FileId = string.IsNullOrEmpty(file.FileIndex) ? file.SessionId.ToString(System.Globalization.CultureInfo.InvariantCulture) : file.FileIndex,
                FileName = file.FileName, FileSize = checked((long)file.FileLength), FileMd5 = Convert.ToHexString(file.FileMd5) });
        }
        return message;
    }
}
