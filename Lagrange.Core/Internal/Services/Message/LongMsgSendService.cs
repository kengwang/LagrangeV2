using System.IO.Compression;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Internal.Events;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Logic;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Message;
using Lagrange.Core.Utility;
using Lagrange.Core.Utility.Extension;

namespace Lagrange.Core.Internal.Services.Message;

[EventSubscribe<LongMsgSendEventReq>(Protocols.All)]
[Service("trpc.group.long_msg_interface.MsgService.SsoSendLongMsg")]
internal class LongMsgSendService : BaseService<LongMsgSendEventReq, LongMsgSendEventResp>
{
    protected override async ValueTask<ReadOnlyMemory<byte>> Build(LongMsgSendEventReq input, BotContext context)
    {
        var messages = new List<CommonMessage>(input.Messages.Count);

        foreach (var msg in input.Messages)
        {
            var fakeMessage = CreateFakeMessage(input.Receiver, msg, context);
            await PreprocessFakeMessage(fakeMessage, context);
            var fakeMsg = await context.EventContext.GetLogic<MessagingLogic>().BuildFake(fakeMessage);
            messages.Add(fakeMsg);
        }

        var content = new PbMultiMsgTransmit
        {
            Items = [new PbMultiMsgItem { FileName = "MultiMsg", Buffer = new PbMultiMsgNew { Msg = messages } }]
        };
        
        await using var dest = new MemoryStream();
        await using var gzip = new GZipStream(dest, CompressionMode.Compress);
        gzip.Write(ProtoHelper.Serialize(content).Span);
        gzip.Close();
        var compressedContent = dest.ToArray();
        
        var longMsg = new LongMsgInterfaceReq
        {
            SendReq = new LongMsgSendReq
            {
                MsgType = input.Receiver is not BotGroup ? 1u : 3u, // 4 for wpamsg, 5 for grpmsg temp
                PeerInfo = new LongMsgPeerInfo { PeerUid = input.Receiver is BotGroup ? input.Receiver.Uid : context.Keystore.Uid },
                GroupUin = input.Receiver is BotGroup group ? group.Uin : 0,
                Payload = compressedContent
            },
            Attr = new LongMsgAttr
            {
                SubCmd = context.Config.Protocol.IsAndroid() ? 3u : 4u, // 1 -> Android 2 -> NTPC 0 -> Undefined
                ClientType = context.Config.Protocol switch
                {
                    Protocols.Windows or Protocols.MacOs or Protocols.Linux => 1u,
                    Protocols.AndroidPhone => 2u,
                    // Protocols.IOS => 3u,
                    // Protocols.IPad => 4u,
                    Protocols.AndroidPad => 5u,
                    _ => 0u
                },
                Platform = context.Config.Protocol switch
                {
                    Protocols.Windows => 3u,
                    Protocols.Linux => 6u,
                    Protocols.MacOs => 7u,
                    // Protocols.IOS => 8u,
                    Protocols.AndroidPad or Protocols.AndroidPhone => 9u,
                    _ => 0u
                },
                ProxyType = 0
            }
        };
        
        return ProtoHelper.Serialize(longMsg);
    }

    private static async Task PreprocessFakeMessage(BotMessage message, BotContext context)
    {
        foreach (var entity in message.Entities)
        {
            await entity.Preprocess(context, message);
        }
    }

    private static BotMessage CreateFakeMessage(BotContact receiver, BotMessage message, BotContext context)
    {
        if (receiver is BotGroup group)
        {
            var fakeGroup = new BotGroup(group.GroupUin, group.GroupName, 0, 0, 0, null, null, null);
            var fakeMemberUid = context.CacheContext.ResolveCachedUid(message.Contact.Uin) ?? message.Contact.Uid;
            var fakeMember = new BotGroupMember(fakeGroup, message.Contact.Uin, fakeMemberUid, message.Contact.Nickname, GroupMemberPermission.Member, 0, message.Contact.Nickname, null, DateTime.Now, DateTime.Now, DateTime.Now);
            return CreateFakeMessage(message, fakeMember, fakeGroup);
        }

        var selfUid = context.Keystore.Uid ?? string.Empty;
        var fakeSender = new BotFriend(message.Contact.Uin, message.Contact.Nickname, selfUid, string.Empty, string.Empty, string.Empty, null!);
        var self = new BotFriend(context.BotUin, string.Empty, selfUid, string.Empty, string.Empty, string.Empty, null!);
        return CreateFakeMessage(message, fakeSender, self);
    }

    private static BotMessage CreateFakeMessage(BotMessage source, BotContact contact, BotContact receiver)
    {
        uint random = (uint)Random.Shared.Next();
        return new BotMessage(source.Entities, contact, receiver, source.Time)
        {
            Random = random,
            Sequence = (ulong)Random.Shared.Next(1000000, 9999999),
            MessageId = (0x01000000ul << 32) | random,
        };
    }

    protected override ValueTask<LongMsgSendEventResp> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        var rsp = ProtoHelper.Deserialize<LongMsgInterfaceRsp>(input.Span);
        return ValueTask.FromResult(new LongMsgSendEventResp(rsp.SendRsp.ResId));
    }
}
