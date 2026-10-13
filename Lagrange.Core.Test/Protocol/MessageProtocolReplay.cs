using System.Collections.Concurrent;
using System.IO.Compression;
using System.Reflection;
using Lagrange.Core;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Internal.Context;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Services;
using Lagrange.Core.Utility;
using Lagrange.Core.Utility.Binary;

namespace Lagrange.Testing;

// Test-only transport: exercise the normal event/service/request matching path
// without sockets, HTTP signing or production injection hooks.
internal sealed class MessageProtocolReplay : IDisposable
{
    public const string LongMessageCommand = "trpc.group.long_msg_interface.MsgService.SsoRecvLongMsg";
    private readonly ConcurrentQueue<BotSsoPacket> _outgoing = new();
    public ConcurrentQueue<BotSsoPacket> Requests { get; } = new();
    public BotContext Bot { get; }

    public MessageProtocolReplay(Func<BotSsoPacket, ReadOnlyMemory<byte>> respond)
    {
        var signer = new ReplaySigner(_outgoing);
        Bot = new BotContext(new BotConfig { SignProvider = signer },
            new BotKeystore { Uin = 200, Uid = "u_self" }, BotAppInfo.ProtocolToAppInfo[Protocols.Linux]);
        Func<ReadOnlyMemory<byte>, CancellationToken, ValueTask> send = (_, _) =>
        {
            if (!_outgoing.TryDequeue(out var request)) throw new InvalidOperationException("Request was not captured.");
            Requests.Enqueue(request);
            var payload = respond(request);
            Bot.PacketContext.DispatchPacket(ResponseFrame(request, payload.Span));
            return ValueTask.CompletedTask;
        };
        typeof(PacketContext).GetField("_send", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(Bot.PacketContext, send);
    }

    public void SeedContacts(params BotContact[] contacts)
    {
        var cache = Bot.CacheContext;
        typeof(CacheContext).GetField("_friends", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(cache, contacts.OfType<BotFriend>().ToList());
        typeof(CacheContext).GetField("_groups", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(cache, contacts.OfType<BotGroup>().ToList());
        var members = (ConcurrentDictionary<long, List<BotGroupMember>>)typeof(CacheContext)
            .GetField("_members", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(cache)!;
        foreach (var group in contacts.OfType<BotGroupMember>().GroupBy(x => x.Group.Uin)) members[group.Key] = group.ToList();
    }

    public static byte[] LongResponse(params CommonMessage[] messages)
    {
        var content = new PbMultiMsgTransmit
        {
            Items = [new PbMultiMsgItem { FileName = "MultiMsg", Buffer = new PbMultiMsgNew { Msg = messages.ToList() } }]
        };
        using var stream = new MemoryStream();
        using (var gzip = new GZipStream(stream, CompressionLevel.Optimal, true))
            gzip.Write(ProtoHelper.Serialize(content).Span);
        return ProtoHelper.Serialize(new LongMsgInterfaceRsp
        {
            RecvRsp = new LongMsgRecvRsp { ResId = "test-forward", Payload = stream.ToArray() }
        }).ToArray();
    }

    public static CommonMessage ForwardNode(string? nestedResId = null) => new()
    {
        RoutingHead = new RoutingHead { FromUin = 100, FromUid = "u_sender", CommonC2C = new() { Name = "snapshot" } },
        ContentHead = new ContentHead { Type = 9, Sequence = 77, ClientSequence = 78, Time = 123 },
        MessageBody = new MessageBody { RichText = new RichText { Elems = nestedResId is null
            ? [new Elem { Text = new Text { TextMsg = "node text" } }]
            : ((Lagrange.Core.Message.Entities.IMessageEntity)new Lagrange.Core.Message.Entities.MultiMsgEntity(nestedResId)).Build().ToList() } }
    };

    private static byte[] ResponseFrame(BotSsoPacket request, ReadOnlySpan<byte> payload)
    {
        using var head = new BinaryPacket(512);
        head.Write(request.Sequence);
        head.Write(0);
        head.Write(string.Empty, Prefix.Int32 | Prefix.WithPrefix);
        head.Write(request.Command, Prefix.Int32 | Prefix.WithPrefix);
        head.Write(ReadOnlySpan<byte>.Empty, Prefix.Int32 | Prefix.WithPrefix);
        head.Write(0);
        head.Write(ReadOnlySpan<byte>.Empty, Prefix.Int32 | Prefix.WithPrefix);
        using var sso = new BinaryPacket(head.CreateReadOnlySpan().Length + payload.Length + 16);
        sso.Write(head.CreateReadOnlySpan(), Prefix.Int32 | Prefix.WithPrefix);
        sso.Write(payload, Prefix.Int32 | Prefix.WithPrefix);
        using var frame = new BinaryPacket(sso.CreateReadOnlySpan().Length + 32);
        frame.EnterLengthBarrier<int>();
        frame.Write(12);
        frame.Write((byte)EncryptType.NoEncrypt);
        frame.Write((byte)0);
        frame.Write("200", Prefix.Int32 | Prefix.WithPrefix);
        frame.Write(sso.CreateReadOnlySpan());
        frame.ExitLengthBarrier<int>(true);
        return frame.CreateReadOnlySpan().ToArray();
    }

    public void Dispose() => Bot.Dispose();

    private sealed class ReplaySigner(ConcurrentQueue<BotSsoPacket> outgoing) : BotSignProvider
    {
        public override bool IsWhiteListCommand(string cmd) => true;
        public override Task<SsoSecureInfo?> GetSecSign(long uin, string cmd, int seq, ReadOnlyMemory<byte> body)
        {
            outgoing.Enqueue(new BotSsoPacket(cmd, body.ToArray(), seq));
            return Task.FromResult<SsoSecureInfo?>(null);
        }
    }
}
