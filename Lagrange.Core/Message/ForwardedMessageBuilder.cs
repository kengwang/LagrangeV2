using Lagrange.Core.Common.Entity;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Message.Entities;

namespace Lagrange.Core.Message;

internal sealed class ForwardedMessageBuilder(BotContext context, BotContact receiver)
{
    private sealed record Prepared(string FileName, string ResId, PbMultiMsgTransmit Content);
    private readonly Dictionary<MultiMsgEntity, Prepared> _prepared = new(ReferenceEqualityComparer.Instance);

    internal async Task<PbMultiMsgTransmit> Build(List<BotMessage> messages)
    {
        // Discover and validate the entire graph before the first upload. Loading
        // independent references is read-only; a failure must not produce a partial card.
        await Validate(messages, new(ReferenceEqualityComparer.Instance), new(ReferenceEqualityComparer.Instance));
        return await Compile(messages);
    }

    private async Task Validate(List<BotMessage> messages, HashSet<MultiMsgEntity> active, HashSet<MultiMsgEntity> visited)
    {
        foreach (var entity in messages.SelectMany(x => x.Entities).OfType<MultiMsgEntity>())
        {
            if (active.Contains(entity)) throw new OperationException(-1, "Cyclic forwarded message tree.");
            if (!visited.Add(entity)) continue;
            active.Add(entity);
            try
            {
                if (!entity.MessagesLoaded && entity.Messages.Count == 0 && !string.IsNullOrEmpty(entity.ResId))
                    entity.SetMessages(await ForwardedMessageResolver.Load(context, receiver is BotGroup, entity.ResId));
                await Validate(entity.Messages, active, visited);
            }
            finally { active.Remove(entity); }
        }
    }

    private async Task<PbMultiMsgTransmit> Compile(List<BotMessage> messages)
    {
        var items = new Dictionary<string, PbMultiMsgItem>(StringComparer.Ordinal);
        foreach (var entity in messages.SelectMany(x => x.Entities).OfType<MultiMsgEntity>())
        {
            if (!_prepared.TryGetValue(entity, out var child))
            {
                var content = await Compile(entity.Messages);
                var result = await context.EventContext.SendEvent<LongMsgSendEventResp>(
                    new LongMsgSendEventReq(receiver, entity.Messages) { PreparedContent = content });
                if (string.IsNullOrEmpty(result.ResId)) throw new OperationException(-1, "Forwarded upload returned an empty resource ID.");
                child = new Prepared(Guid.NewGuid().ToString(), result.ResId, content);
                _prepared.Add(entity, child);
            }
            foreach (var item in child.Content.Items)
            {
                string name = item.FileName == "MultiMsg" ? child.FileName : item.FileName;
                items.TryAdd(name, new PbMultiMsgItem { FileName = name, Buffer = item.Buffer });
            }
        }

        var packer = new MessagePacker(context);
        var nodes = new List<CommonMessage>(messages.Count);
        foreach (var message in messages)
            nodes.Add(await packer.BuildFake(message, entity =>
            {
                var child = _prepared[entity];
                return entity.BuildCard(child.ResId, child.FileName, entity.Messages.Count);
            }));
        return new PbMultiMsgTransmit
        {
            Items = [new PbMultiMsgItem { FileName = "MultiMsg", Buffer = new() { Msg = nodes } }, .. items.Values]
        };
    }
}
