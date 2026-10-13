using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Message.Entities;

namespace Lagrange.Core.Message;

// A scope follows nested service calls, but never leaks into another message's
// asynchronous execution. Each downloaded package has its own filename index.
internal sealed class ForwardedMessageResolver(BotContext context)
{
    private readonly BotContext _context = context;
    private static readonly AsyncLocal<ForwardedMessageResolver?> Current = new();
    private readonly Dictionary<(bool Group, string ResId), List<BotMessage>> _downloads = [];
    private readonly HashSet<(bool Group, string ResId)> _activeDownloads = [];

    internal static async Task<List<BotMessage>> Load(BotContext context, bool isGroup, string resId)
    {
        var previous = Current.Value;
        var resolver = previous ?? new ForwardedMessageResolver(context);
        Current.Value = resolver;
        try { return await resolver.Download(isGroup, resId); }
        finally { Current.Value = previous; }
    }

    private async Task<List<BotMessage>> Download(bool isGroup, string resId)
    {
        var key = (isGroup, resId);
        if (_downloads.TryGetValue(key, out var messages)) return messages;
        if (!_activeDownloads.Add(key)) throw new OperationException(-1, "Cyclic forwarded resource reference.");
        try
        {
            var result = await _context.EventContext.SendEvent<LongMsgRecvEventResp>(new LongMsgRecvEventReq(isGroup, resId));
            _downloads[key] = result.Messages;
            return result.Messages;
        }
        finally { _activeDownloads.Remove(key); }
    }

    internal static async Task<List<BotMessage>> Parse(BotContext context, PbMultiMsgTransmit content)
    {
        var previous = Current.Value;
        var resolver = previous ?? new ForwardedMessageResolver(context);
        Current.Value = resolver;
        try { return await new Package(resolver, content).Resolve("MultiMsg"); }
        finally { Current.Value = previous; }
    }

    private sealed class Package
    {
        private readonly ForwardedMessageResolver _resolver;
        private readonly Dictionary<string, PbMultiMsgNew> _items = new(StringComparer.Ordinal);
        private readonly Dictionary<string, List<BotMessage>> _parsed = new(StringComparer.Ordinal);
        private readonly HashSet<string> _active = new(StringComparer.Ordinal);

        public Package(ForwardedMessageResolver resolver, PbMultiMsgTransmit content)
        {
            _resolver = resolver;
            foreach (var item in content.Items ?? [])
                if (string.IsNullOrEmpty(item.FileName) || item.Buffer?.Msg is null || !_items.TryAdd(item.FileName, item.Buffer))
                    throw new OperationException(-1, "Invalid or duplicate forwarded item filename.");
            if (!_items.ContainsKey("MultiMsg")) throw new OperationException(-1, "Forwarded payload omitted MultiMsg.");
        }

        public async Task<List<BotMessage>> Resolve(string fileName)
        {
            if (_parsed.TryGetValue(fileName, out var cached)) return cached;
            if (!_active.Add(fileName)) throw new OperationException(-1, "Cyclic forwarded item reference.");
            try
            {
                var packer = new MessagePacker(_resolver._context);
                var messages = new List<BotMessage>();
                foreach (var msg in _items[fileName].Msg)
                    messages.Add(await packer.ParseForwarded(msg, ResolveEntity));
                _parsed[fileName] = messages;
                return messages;
            }
            finally { _active.Remove(fileName); }
        }

        private async Task ResolveEntity(MultiMsgEntity entity, BotMessage owner)
        {
            if (_items.ContainsKey(entity.FileName)) entity.SetMessages(await Resolve(entity.FileName));
            else if (!string.IsNullOrEmpty(entity.ResId))
                entity.SetMessages(await _resolver.Download(owner.Contact is Lagrange.Core.Common.Entity.BotGroupMember, entity.ResId));
            else throw new OperationException(-1, "Forwarded node has neither an embedded item nor a resource ID.");
        }
    }
}
