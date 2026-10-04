using System.Text.Json;
using System.Text.RegularExpressions;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Packets.Notify;
using Lagrange.Core.Utility;
using Lagrange.Core.Utility.Binary;

namespace Lagrange.Core.Internal.Logic.Processors;

[MsgPushProcessor(MsgType.Event0x2DC, 16, true)]
internal sealed class GroupExtendedEventProcessor : MsgPushProcessorBase
{
    internal override ValueTask<bool> Handle(BotContext context, MsgType msgType, int subType,
        PushMessageEvent msgEvt, ReadOnlyMemory<byte>? content)
    {
        if (content is null) return ValueTask.FromResult(false);
        var notify = ReadNotify(content.Value.Span);
        if (notify is null || notify.SubType != 6 || notify.EventParam is null) return ValueTask.FromResult(false);

        var title = ProtoHelper.Deserialize<GroupSpecialTitleChange>(notify.EventParam);
        var parsedTitle = ExtractTitle(title.TipText ?? string.Empty);
        if (title.MemberUin == 0 || string.IsNullOrWhiteSpace(parsedTitle)) return ValueTask.FromResult(false);

        var operatorUin = string.IsNullOrWhiteSpace(notify.OperatorUid)
            ? 0
            : context.CacheContext.ResolveUin(notify.OperatorUid);
        context.EventInvoker.PostEvent(new BotGroupSpecialTitleChangeEvent(
            notify.GroupUin, title.MemberUin, operatorUin, parsedTitle));
        return ValueTask.FromResult(true);
    }

    private static NotifyMessageBody? ReadNotify(ReadOnlySpan<byte> bytes)
    {
        foreach (var prefix in new[] { 5, 7 })
        {
            try
            {
                var packet = new BinaryPacket(bytes);
                if (bytes.Length <= prefix) continue;
                packet.Skip(prefix);
                return ProtoHelper.Deserialize<NotifyMessageBody>(packet.ReadBytes(Prefix.Int16 | Prefix.LengthOnly));
            }
            catch (Exception) { }
        }
        return null;
    }

    private static string ExtractTitle(string text)
    {
        var matches = Regex.Matches(text, "<\\{[^<>]*\\}>");
        if (matches.Count == 0) return string.Empty;
        var value = matches[^1].Value;
        try
        {
            using var json = JsonDocument.Parse(value[1..^1]);
            return json.RootElement.TryGetProperty("text", out var title) ? title.GetString() ?? string.Empty : string.Empty;
        }
        catch (JsonException) { return string.Empty; }
    }
}
