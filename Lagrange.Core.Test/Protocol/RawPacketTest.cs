using System.Collections.Concurrent;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Internal.Context;
using Lagrange.Core.Internal.Packets.Struct;
using Lagrange.Core.Services;
using Lagrange.Core.Utility.Binary;
using Lagrange.Core.Utility.Compression;

namespace Lagrange.Core.Test.Protocol;

public sealed class RawPacketTest
{
    private static BotContext Bot(BotSignProvider? signer = null) =>
        BotFactory.Create(new BotConfig { SignProvider = signer }, new BotKeystore(), BotAppInfo.ProtocolToAppInfo[Protocols.Linux]);

    private static BotSsoPacket Packet(int sequence) => new("Unknown.RawCommand", new byte[] { 8, 1, 0xAB, 0 }, sequence);

    // Server SSO headers differ from client request headers. Build actual receive frames.
    private static byte[] Response(int sequence, int retCode = 0, int dataFlag = 0, string extra = "details")
    {
        using var head = new BinaryPacket(256);
        head.Write(sequence);
        head.Write(retCode);
        head.Write(extra, Prefix.Int32 | Prefix.WithPrefix);
        head.Write("Unknown.RawCommand", Prefix.Int32 | Prefix.WithPrefix);
        head.Write(ReadOnlySpan<byte>.Empty, Prefix.Int32 | Prefix.WithPrefix);
        head.Write(dataFlag);
        head.Write(ReadOnlySpan<byte>.Empty, Prefix.Int32 | Prefix.WithPrefix);
        byte[] payload = [8, 1, 0xAB, 0];
        if (dataFlag == 1) payload = ZCompression.ZCompress(payload);
        using var sso = new BinaryPacket(512);
        sso.Write(head.CreateReadOnlySpan(), Prefix.Int32 | Prefix.WithPrefix);
        sso.Write(payload, Prefix.Int32 | Prefix.WithPrefix);
        return sso.CreateReadOnlySpan().ToArray();
    }

    private static byte[] Frame(byte[] sso)
    {
        using var frame = new BinaryPacket(sso.Length + 32);
        frame.EnterLengthBarrier<int>();
        frame.Write(12);
        frame.Write((byte)EncryptType.NoEncrypt);
        frame.Write((byte)0);
        frame.Write("123", Prefix.Int32 | Prefix.WithPrefix);
        frame.Write(sso);
        frame.ExitLengthBarrier<int>(true);
        return frame.CreateReadOnlySpan().ToArray();
    }

    [TestCase(0, 0)]
    [TestCase(7, 0)]
    [TestCase(0, 1)]
    [TestCase(7, 1)]
    [TestCase(0, 4)]
    public void ParserPreservesPayloadReturnCodeAndExtra(int retCode, int dataFlag)
    {
        using var bot = Bot();
        var packet = new SsoPacker(bot).Parse(Response(123, retCode, dataFlag));
        Assert.Multiple(() =>
        {
            Assert.That(packet.Command, Is.EqualTo("Unknown.RawCommand"));
            Assert.That(packet.Sequence, Is.EqualTo(123));
            Assert.That(packet.RetCode, Is.EqualTo(retCode));
            Assert.That(packet.Extra, Is.EqualTo("details"));
            Assert.That(packet.Data.ToArray(), Is.EqualTo(new byte[] { 8, 1, 0xAB, 0 }));
        });
    }

    [TestCase(0)]
    [TestCase(7)]
    public async Task RawResponseIsPublishedOnceAndReturnedUnchanged(int retCode)
    {
        using var bot = Bot();
        var events = new EventRecorder();
        bot.EventInvoker.RegisterEvent<BotPacketReceivedEvent>(events.Record);
        PacketContext? packets = null;
        int writes = 0;
        using (packets = new PacketContext(bot, (_, _) =>
        {
            Interlocked.Increment(ref writes);
            packets!.DispatchPacket(Frame(Response(11, retCode, 1)));
            return ValueTask.CompletedTask;
        }))
        {
            var response = await packets.SendRawPacket(Packet(11), true, default);
            var received = await events.First.Task.WaitAsync(TimeSpan.FromSeconds(3));
            Assert.Multiple(() =>
            {
                Assert.That(response!.RetCode, Is.EqualTo(retCode));
                Assert.That(response.Extra, Is.EqualTo("details"));
                Assert.That(received.Data.ToArray(), Is.EqualTo(response.Data.ToArray()));
                Assert.That(events.Count, Is.EqualTo(1));
                Assert.That(writes, Is.EqualTo(1));
                Assert.That(packets.PendingCount, Is.Zero);
            });
        }
    }

    [Test]
    public async Task LegacyNonzeroResponseStillThrowsAndEventHandlerCannotBreakMatching()
    {
        using var bot = Bot();
        var events = new EventRecorder();
        bot.EventInvoker.RegisterEvent<BotPacketReceivedEvent>(events.Throw);
        PacketContext? packets = null;
        using (packets = new PacketContext(bot, (_, _) =>
        {
            packets!.DispatchPacket(Frame(Response(12, 7)));
            return ValueTask.CompletedTask;
        }))
        {
            var pending = packets.SendPacket(Packet(12), new ServiceAttribute("Unknown.RawCommand", RequestType.D2Auth, EncryptType.EncryptD2Key));
            Assert.ThrowsAsync<InvalidOperationException>(async () => await pending.AsTask().WaitAsync(TimeSpan.FromSeconds(3)));
            await events.First.Task.WaitAsync(TimeSpan.FromSeconds(3));
            Assert.That(packets.PendingCount, Is.Zero);
        }
    }

    [Test]
    public async Task NoResponseModeWaitsForTheWriteAndCreatesNoPendingEntry()
    {
        using var bot = Bot();
        var written = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var packets = new PacketContext(bot, async (_, token) => await written.Task.WaitAsync(token));
        var send = packets.SendRawPacket(Packet(21), false, default).AsTask();
        Assert.That(send.IsCompleted, Is.False);
        Assert.That(packets.PendingCount, Is.Zero);
        written.SetResult();
        Assert.That(await send, Is.Null);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void WriteFailurePropagatesAndCleansUp(bool waitForResponse)
    {
        using var bot = Bot();
        using var packets = new PacketContext(bot, (_, _) => throw new IOException("write failed"));
        Assert.ThrowsAsync<IOException>(async () => await packets.SendRawPacket(Packet(22), waitForResponse, default));
        Assert.That(packets.PendingCount, Is.Zero);
    }

    [TestCase("cancel")]
    [TestCase("timeout")]
    [TestCase("disconnect")]
    [TestCase("dispose")]
    public async Task InterruptedWaitIsCleanedUpAndLateResponseStillRaisesEvent(string mode)
    {
        using var bot = Bot();
        var events = new EventRecorder();
        bot.EventInvoker.RegisterEvent<BotPacketReceivedEvent>(events.Record);
        int writes = 0;
        using var packets = new PacketContext(bot, (_, _) => { writes++; return ValueTask.CompletedTask; });
        using var token = new CancellationTokenSource();
        var pending = packets.SendRawPacket(Packet(31), true, token.Token).AsTask();
        if (mode == "cancel") token.Cancel();
        if (mode == "timeout") token.CancelAfter(TimeSpan.FromMilliseconds(30));
        if (mode == "disconnect") packets.OnDisconnect();
        if (mode == "dispose") packets.Dispose();
        if (mode is "disconnect" or "dispose")
            Assert.ThrowsAsync<IOException>(async () => await pending.WaitAsync(TimeSpan.FromSeconds(3)));
        else
            Assert.That(async () => await pending.WaitAsync(TimeSpan.FromSeconds(3)), Throws.InstanceOf<OperationCanceledException>());
        Assert.That(packets.PendingCount, Is.Zero);
        packets.DispatchPacket(Frame(Response(31)));
        await events.First.Task.WaitAsync(TimeSpan.FromSeconds(3));
        Assert.That(writes, Is.EqualTo(1));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task CancellationDuringSigningPreventsSocketWrite(bool waitForResponse)
    {
        var signer = new BlockingSigner();
        using var bot = Bot(signer);
        int writes = 0;
        using var packets = new PacketContext(bot, (_, _) => { writes++; return ValueTask.CompletedTask; });
        using var cancellation = new CancellationTokenSource();
        var pending = packets.SendRawPacket(Packet(41), waitForResponse, cancellation.Token).AsTask();
        await signer.Started.Task.WaitAsync(TimeSpan.FromSeconds(3));
        cancellation.Cancel();
        Assert.That(async () => await pending, Throws.InstanceOf<OperationCanceledException>());
        signer.Result.SetResult(null);
        Assert.That(writes, Is.Zero);
        Assert.That(packets.PendingCount, Is.Zero);
    }

    [Test]
    public async Task ConcurrentResponsesMatchSequenceEvenWhenReversed()
    {
        using var bot = Bot();
        int writes = 0;
        using var packets = new PacketContext(bot, (_, _) => { Interlocked.Increment(ref writes); return ValueTask.CompletedTask; });
        var pending = Enumerable.Range(100, 32).Select(seq => packets.SendRawPacket(Packet(seq), true, default).AsTask()).ToArray();
        foreach (int seq in Enumerable.Range(100, 32).Reverse()) packets.DispatchPacket(Frame(Response(seq, seq)));
        var responses = await Task.WhenAll(pending).WaitAsync(TimeSpan.FromSeconds(3));
        Assert.That(responses.Select(x => x!.Sequence), Is.EqualTo(Enumerable.Range(100, 32)));
        Assert.That(responses.Select(x => x!.RetCode), Is.EqualTo(Enumerable.Range(100, 32)));
        Assert.That(writes, Is.EqualTo(32));
        Assert.That(packets.PendingCount, Is.Zero);
    }

    [Test]
    public async Task ResponseAndCancellationRacesNeverLeavePendingRequests()
    {
        using var bot = Bot();
        using var packets = new PacketContext(bot, (_, _) => ValueTask.CompletedTask);
        for (int seq = 200; seq < 232; seq++)
        {
            using var cancellation = new CancellationTokenSource();
            var pending = packets.SendRawPacket(Packet(seq), true, cancellation.Token).AsTask();
            await Task.WhenAll(Task.Run(() => cancellation.Cancel()), Task.Run(() => packets.DispatchPacket(Frame(Response(seq)))));
            try { await pending.WaitAsync(TimeSpan.FromSeconds(3)); }
            catch (OperationCanceledException) { }
            Assert.That(packets.PendingCount, Is.Zero);
        }
    }

    [Test]
    public async Task PublicApiUsesFreshSequencesAndFailsWhenOffline()
    {
        var signer = new CapturingSigner();
        using var bot = Bot(signer);
        var packet = Packet(999);
        for (int i = 0; i < 2; i++)
            Assert.ThrowsAsync<IOException>(async () => await bot.SendRawPacket(packet, false));
        Assert.That(signer.Sequences.Distinct().Count(), Is.EqualTo(2));
        Assert.That(signer.Sequences, Does.Not.Contain(999));
        Assert.That(bot.PacketContext.PendingCount, Is.Zero);
        using var canceled = new CancellationTokenSource();
        canceled.Cancel();
        Assert.Throws<OperationCanceledException>(() => bot.SendRawPacket(packet, cancellationToken: canceled.Token));
        Assert.Throws<ArgumentNullException>(() => bot.SendRawPacket(null!));
        Assert.Throws<ArgumentException>(() => bot.SendRawPacket(new BotSsoPacket(" ", ReadOnlyMemory<byte>.Empty)));
        await Task.CompletedTask;
    }

    [Test]
    public void SigningFailureCleansUpWithoutWriting()
    {
        using var bot = Bot(new FailingSigner());
        int writes = 0;
        using var packets = new PacketContext(bot, (_, _) => { writes++; return ValueTask.CompletedTask; });
        Assert.ThrowsAsync<InvalidOperationException>(async () => await packets.SendRawPacket(Packet(51), true, default));
        Assert.That(packets.PendingCount, Is.Zero);
        Assert.That(writes, Is.Zero);
    }

    [Test]
    public async Task SocketWritesConcurrentLargeFramesCompletelyWithoutInterleaving()
    {
        using var bot = Bot();
        using var server = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        server.Start();
        var accepted = server.AcceptTcpClientAsync();
        Assert.That(await bot.SocketContext.Connect("127.0.0.1", (ushort)((System.Net.IPEndPoint)server.LocalEndpoint).Port), Is.True);
        using var peer = await accepted;
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        byte[] first = Enumerable.Repeat((byte)1, 1024 * 1024).ToArray();
        byte[] second = Enumerable.Repeat((byte)2, 1024 * 1024).ToArray();
        byte[] received = new byte[first.Length + second.Length];
        var writes = Task.WhenAll(bot.SocketContext.SendFully(first, deadline.Token).AsTask(), bot.SocketContext.SendFully(second, deadline.Token).AsTask());
        await peer.GetStream().ReadExactlyAsync(received, deadline.Token);
        await writes;
        Assert.That(received.AsSpan(0, first.Length).ToArray(), Is.EqualTo(first));
        Assert.That(received.AsSpan(first.Length).ToArray(), Is.EqualTo(second));
    }

    private sealed class FailingSigner : BotSignProvider
    {
        public override bool IsWhiteListCommand(string cmd) => true;
        public override Task<SsoSecureInfo?> GetSecSign(long uin, string cmd, int seq, ReadOnlyMemory<byte> body) =>
            Task.FromException<SsoSecureInfo?>(new InvalidOperationException("signing failed"));
    }

    private sealed class EventRecorder
    {
        public readonly TaskCompletionSource<BotPacketReceivedEvent> First = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int _count;
        public int Count => Volatile.Read(ref _count);
        public void Record(BotContext bot, BotPacketReceivedEvent packet)
        {
            Interlocked.Increment(ref _count);
            First.TrySetResult(packet);
        }
        public void Throw(BotContext bot, BotPacketReceivedEvent packet)
        {
            Record(bot, packet);
            throw new InvalidOperationException("consumer failed");
        }
    }

    private sealed class BlockingSigner : BotSignProvider
    {
        public readonly TaskCompletionSource Started = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public readonly TaskCompletionSource<SsoSecureInfo?> Result = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public override bool IsWhiteListCommand(string cmd) => true;
        public override Task<SsoSecureInfo?> GetSecSign(long uin, string cmd, int seq, ReadOnlyMemory<byte> body)
        {
            Started.TrySetResult();
            return Result.Task;
        }
    }

    private sealed class CapturingSigner : BotSignProvider
    {
        public readonly ConcurrentQueue<int> Sequences = new();
        public override bool IsWhiteListCommand(string cmd) => true;
        public override Task<SsoSecureInfo?> GetSecSign(long uin, string cmd, int seq, ReadOnlyMemory<byte> body)
        {
            Sequences.Enqueue(seq);
            return Task.FromResult<SsoSecureInfo?>(null);
        }
    }
}
