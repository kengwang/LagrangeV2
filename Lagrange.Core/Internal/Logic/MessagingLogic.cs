using Lagrange.Core.Common.Entity;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Message;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Internal.Logic;

internal class MessagingLogic(BotContext context) : ILogic
{
    private readonly MessagePacker _packer = new(context);

    public Task<BotMessage> Parse(CommonMessage msg) => _packer.Parse(msg);

    public Task<CommonMessage> BuildFake(BotMessage msg) => _packer.BuildFake(msg);

    public async Task<List<BotMessage>> GetGroupMessage(long groupUin, ulong startSequence, ulong endSequence, CancellationToken cancellationToken = default)
    {
        var result = await context.EventContext.SendEvent<GetGroupMessageEventResp>(new GetGroupMessageEventReq(groupUin, startSequence, endSequence), cancellationToken);
        var messages = new List<BotMessage>(result.Chains.Count);
        foreach (var chain in result.Chains) messages.Add(await Parse(chain));
        return messages;
    }

    public async Task<List<BotMessage>> GetRoamMessage(long peerUin, uint time, uint count, CancellationToken cancellationToken = default)
    {
        string peerUid = context.CacheContext.ResolveCachedUid(peerUin) ?? throw new InvalidTargetException(peerUin);
        var result = await context.EventContext.SendEvent<GetRoamMessageEventResp>(new GetRoamMessageEventReq(peerUid, time, count), cancellationToken);
        var messages = new List<BotMessage>(result.Chains.Count);
        foreach (var chain in result.Chains) messages.Add(await Parse(chain));
        return messages;
    }

    public async Task<List<BotMessage>> GetC2CMessage(long peerUin, ulong startSequence, ulong endSequence, CancellationToken cancellationToken = default)
    {
        string peerUid = context.CacheContext.ResolveCachedUid(peerUin) ?? throw new InvalidTargetException(peerUin);
        var result = await context.EventContext.SendEvent<GetC2CMessageEventResp>(new GetC2CMessageEventReq(peerUid, startSequence, endSequence), cancellationToken);
        var messages = new List<BotMessage>(result.Chains.Count);
        foreach (var chain in result.Chains) messages.Add(await Parse(chain));
        return messages;
    }

    public async Task<BotMessage> SendFriendMessage(long friendUin, MessageChain chain)
    {
        ValidatePoke(chain, false);
        var friend = await context.CacheContext.ResolveFriend(friendUin) ?? throw new InvalidTargetException(friendUin);
        var self = await context.CacheContext.ResolveFriend(context.BotUin) ?? throw new InvalidTargetException(context.BotUin);
        var message = await BuildMessage(chain, self, friend);
        var result = await context.EventContext.SendEvent<SendMessageEventResp>(new SendMessageEventReq(message));

        if (result == null) throw new InvalidOperationException();
        if (result.Result != 0) throw new OperationException(result.Result);

        message.Sequence = result.Sequence;
        message.Time = result.SendTime;

        return message;
    }

    public async Task<BotMessage> SendGroupMessage(long groupUin, MessageChain chain)
    {
        ValidatePoke(chain, true);
        var (group, self) = await context.CacheContext.ResolveMember(groupUin, context.BotUin) ?? throw new InvalidTargetException(context.BotUin, groupUin);
        var message = await BuildMessage(chain, self, group);
        var result = await context.EventContext.SendEvent<SendMessageEventResp>(new SendMessageEventReq(message));

        if (result == null) throw new InvalidOperationException();
        if (result.Result != 0) throw new OperationException(result.Result);

        message.Sequence = result.Sequence;
        message.Time = result.SendTime;

        return message;
    }

    public async Task RecallMessage(BotMessage message, CancellationToken cancellationToken = default)
    {
        switch (message.Contact)
        {
            case BotGroupMember member:
                await context.EventContext.SendEvent<GroupRecallMsgEventResp>(new GroupRecallMsgEventReq(member.Group.GroupUin, message.Sequence), cancellationToken);
                break;
            case BotFriend friend:
                await context.EventContext.SendEvent<C2CRecallMsgEventResp>(new C2CRecallMsgEventReq(friend.Uin == context.BotUin ? message.Receiver.Uid : friend.Uid, message.Sequence, message.ClientSequence, message.Random, (uint)message.Time), cancellationToken);
                break;
            default: throw new InvalidTargetException(message.Contact.Uin);
        }
    }

    public Task SetEssenceMessage(BotMessage message, CancellationToken cancellationToken = default)
    {
        if (message.Contact is not BotGroupMember member) throw new ArgumentException("Only group messages can be set as essence messages.", nameof(message));

        return SetEssenceMessage(member.Group.GroupUin, message.Sequence, message.Random, cancellationToken);
    }

    public Task RemoveEssenceMessage(BotMessage message, CancellationToken cancellationToken = default)
    {
        if (message.Contact is not BotGroupMember member) throw new ArgumentException("Only group messages can be removed from essence messages.", nameof(message));

        return RemoveEssenceMessage(member.Group.GroupUin, message.Sequence, message.Random, cancellationToken);
    }

    public async Task SetEssenceMessage(long groupUin, ulong sequence, uint random, CancellationToken cancellationToken = default)
    {
        await context.EventContext.SendEvent<SetEssenceMessageEventResp>(new SetEssenceMessageEventReq(groupUin, sequence, random), cancellationToken);
    }

    public async Task RemoveEssenceMessage(long groupUin, ulong sequence, uint random, CancellationToken cancellationToken = default)
    {
        await context.EventContext.SendEvent<RemoveEssenceMessageEventResp>(new RemoveEssenceMessageEventReq(groupUin, sequence, random), cancellationToken);
    }

    public async Task<BotMessage> SendTempMessage(long groupUin, long userUin, MessageChain chain, CancellationToken ct)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(groupUin);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userUin);
        ArgumentNullException.ThrowIfNull(chain);
        if (chain.Count == 0) throw new ArgumentException("Message is empty.", nameof(chain));
        if (chain.Any(x => x is Lagrange.Core.Message.Entities.PokeEntity)) throw new ArgumentException("Window shake requires a direct private chat.", nameof(chain));
        ct.ThrowIfCancellationRequested();
        var target = await context.CacheContext.ResolveStranger(userUin).WaitAsync(ct);
        var self = new BotFriend(context.BotUin, context.BotInfo?.Name ?? string.Empty, context.Keystore.Uid, string.Empty, string.Empty, string.Empty, null!);
        var random = (uint)Random.Shared.Next();
        var message = new BotMessage(chain, self, target, DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        {
            TempGroupUin = groupUin, Random = random, MessageId = (0x10000000ul << 32) | random
        };
        foreach (var entity in chain) { ct.ThrowIfCancellationRequested(); await entity.Preprocess(context, message).WaitAsync(ct); }
        var result = await context.EventContext.SendEvent<SendMessageEventResp>(new SendMessageEventReq(message), ct);
        if (result.Result != 0) throw new OperationException(result.Result, result.ErrorMessage);
        message.Sequence = result.Sequence;
        message.Time = result.SendTime;
        return message;
    }

    internal static void ValidatePoke(MessageChain chain, bool group)
    {
        ArgumentNullException.ThrowIfNull(chain);
        if (chain.Any(x => x is Lagrange.Core.Message.Entities.PokeEntity) && (group || chain.Count != 1))
            throw new ArgumentException("Window shake must be the only element in a direct private message.", nameof(chain));
    }

    private async Task<BotMessage> BuildMessage(MessageChain chain, BotContact contact, BotContact receiver)
    {
        uint random = (uint)Random.Shared.Next();
        var message = new BotMessage(chain, contact, receiver, DateTimeOffset.Now.ToUnixTimeSeconds())
        {
            Random = random,
            MessageId = (0x10000000ul << 32) | random
        };

        foreach (var entity in chain)
        {
            await entity.Preprocess(context, message);
        }

        return message;
    }
}
