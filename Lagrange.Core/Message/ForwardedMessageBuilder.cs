using Lagrange.Core.Common.Entity;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Message.Entities;

namespace Lagrange.Core.Message;

internal sealed class ForwardedMessageBuilder(BotContext context, BotContact receiver, long? tempGroupUin = null)
{
    private sealed record Prepared(string FileName, string ResId, PbMultiMsgTransmit Content);
    private readonly Dictionary<MultiMsgEntity, Prepared> _prepared = new(ReferenceEqualityComparer.Instance);

    private readonly Dictionary<BotMessage, BotMessage> _nodes = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<BotMessage, string> _paths = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<IMessageEntity, string> _locations = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<IMessageEntity, IMessageEntity> _entities = new(ReferenceEqualityComparer.Instance);

    internal async Task<PbMultiMsgTransmit> Build(List<BotMessage> messages)
    {
        Exception? failure = null;
        try
        {
            await Validate(messages, new(ReferenceEqualityComparer.Instance), new(ReferenceEqualityComparer.Instance));
            await Prepare(messages, "root");
            return await Compile(messages);
        }
        catch (Exception e) { failure = e; throw; }
        finally
        {
            foreach (var media in CollectMedia(messages, new(ReferenceEqualityComparer.Instance)))
            {
                try { await media.ReleaseOwnedResourcesAsync(); }
                catch (Exception e) when (failure is not null)
                { context.LogWarning(nameof(ForwardedMessageBuilder), "Media cleanup also failed: {0}", null, e.Message); }
            }
        }
    }

    private static IEnumerable<RichMediaEntityBase> CollectMedia(List<BotMessage> messages, HashSet<IMessageEntity> visited)
    {
        foreach (var entity in messages.SelectMany(x => x.Entities))
        {
            if (!visited.Add(entity)) continue;
            if (entity is RichMediaEntityBase media) yield return media;
            if (entity is MultiMsgEntity child)
                foreach (var nested in CollectMedia(child.Messages, visited)) yield return nested;
        }
    }

    private BotMessage TransportMessage(MessageChain chain)
    {
        BotContact sender = receiver is BotGroup group
            ? new BotGroupMember(group, context.BotUin, context.Keystore.Uid, context.BotInfo?.Name ?? "", GroupMemberPermission.Member, 0, null, null, 0, 0, 0)
            : new BotFriend(context.BotUin, context.BotInfo?.Name ?? "", context.Keystore.Uid, "", "", "", null!);
        return new BotMessage(chain, sender, receiver, DateTimeOffset.UtcNow.ToUnixTimeSeconds()) { TempGroupUin = tempGroupUin };
    }

    private async Task Prepare(List<BotMessage> messages, string path)
    {
        for (int i = 0; i < messages.Count; i++)
        {
            var original = messages[i];
            if (_nodes.ContainsKey(original)) continue;
            _paths.Add(original, $"{path}[{i}]");
            var chain = new MessageChain { Style = original.Style };
            for (int j = 0; j < original.Entities.Count; j++)
            {
                var entity = original.Entities[j];
                var location = $"{path}[{i}].segments[{j}]";
                _locations.TryAdd(entity, location);
                try
                {
                    if (!_entities.TryGetValue(entity, out var prepared))
                    {
                        if (entity is MultiMsgEntity forward)
                            await Prepare(forward.Messages, location + ".messages");
                        else if (entity is MentionEntity mention)
                        {
                            prepared = await mention.PrepareForward(context, receiver);
                            _entities.Add(entity, prepared);
                        }
                        else await entity.Preprocess(context, TransportMessage(original.Entities));
                        if (!_entities.ContainsKey(entity)) _entities.Add(entity, entity);
                    }
                    chain.Add(_entities[entity]);
                }
                catch (Exception e) when (e is not ForwardPreparationException && e is not OperationCanceledException)
                { throw new ForwardPreparationException(location, entity.GetType().Name, e); }
            }
            _nodes.Add(original, new BotMessage(chain, original.Contact, original.Receiver, original.Time)
            {
                Random = original.Random, MessageId = original.MessageId, Sequence = original.Sequence,
                ClientSequence = original.ClientSequence, TempGroupUin = original.TempGroupUin
            });
        }
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
            try
            {
                if (!_prepared.TryGetValue(entity, out var child))
                {
                    var content = await Compile(entity.Messages);
                    var result = await context.EventContext.SendEvent<LongMsgSendEventResp>(
                        new LongMsgSendEventReq(receiver, entity.Messages) { PreparedContent = content, TempGroupUin = tempGroupUin });
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
            catch (Exception e) when (e is not ForwardPreparationException && e is not OperationCanceledException)
            { throw new ForwardPreparationException(_locations[entity], "forward upload", e); }
        }

        var packer = new MessagePacker(context);
        var nodes = new List<CommonMessage>(messages.Count);
        foreach (var message in messages)
        {
            try
            {
                nodes.Add(await packer.BuildFake(_nodes[message], entity =>
                {
                    var child = _prepared[entity];
                    return entity.BuildCard(child.ResId, child.FileName, entity.Messages.Count);
                }));
            }
            catch (Exception e) when (e is not ForwardPreparationException && e is not OperationCanceledException)
            { throw new ForwardPreparationException(_paths[message], "packing", e); }
        }
        return new PbMultiMsgTransmit
        {
            Items = [new PbMultiMsgItem { FileName = "MultiMsg", Buffer = new() { Msg = nodes } }, .. items.Values]
        };
    }
}
