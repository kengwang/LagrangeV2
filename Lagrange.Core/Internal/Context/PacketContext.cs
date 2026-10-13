using System.Collections.Concurrent;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Internal.Packets.Struct;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Context;

internal class PacketContext : IDisposable
{
    private sealed class PendingPacket(bool raw)
    {
        public bool Raw { get; } = raw;
        public TaskCompletionSource<BotSsoPacket> Completion { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private readonly ConcurrentDictionary<int, PendingPacket> _pendingTasks = new();
    private readonly object _lifetimeLock = new();
    private CancellationTokenSource _rawLifetime = new();
    private bool _disposed;
    private readonly Func<ReadOnlyMemory<byte>, CancellationToken, ValueTask> _send;

    private readonly BotKeystore _keystore;
    private readonly SsoPacker _ssoPacker;
    private readonly ServicePacker _servicePacker;

    internal readonly BotSignProvider SignProvider;

    private readonly BotContext _context;

    public PacketContext(BotContext context, Func<ReadOnlyMemory<byte>, CancellationToken, ValueTask>? send = null)
    {
        _context = context;
        _keystore = context.Keystore;
        _ssoPacker = new SsoPacker(context);
        _servicePacker = new ServicePacker(context);
        _send = send ?? ((frame, token) => context.SocketContext.SendFully(frame, token));

        SignProvider = context.Config.SignProvider ?? context.Config.Protocol switch
        {
            Protocols.Linux or Protocols.Windows or Protocols.MacOs => new DefaultBotSignProvider(),
            Protocols.AndroidPhone or Protocols.AndroidPad => new DefaultAndroidBotSignProvider(),
            _ => throw new ArgumentOutOfRangeException(nameof(context.Config.Protocol))
        };

        SignProvider.Context = context; // Initialize the sign provider with the context
    }

    public ValueTask<BotSsoPacket> SendPacket(BotSsoPacket packet, ServiceAttribute options)
    {
        var pending = new PendingPacket(false);
        if (!_pendingTasks.TryAdd(packet.Sequence, pending))
            throw new InvalidOperationException($"Packet sequence {packet.Sequence} is already pending.");
        Task.Run(async () => // Schedule the task to the ThreadPool
        {
            try
            {
                await SendFrame(packet, options, CancellationToken.None);
            }
            catch (Exception e)
            {
                if (_pendingTasks.TryRemove(new KeyValuePair<int, PendingPacket>(packet.Sequence, pending)))
                    pending.Completion.TrySetException(e);
            }
        });

        return new ValueTask<BotSsoPacket>(pending.Completion.Task);
    }

    public async ValueTask<BotSsoPacket?> SendRawPacket(BotSsoPacket packet, bool waitForResponse, CancellationToken cancellationToken)
    {
        CancellationTokenSource lifetime;
        lock (_lifetimeLock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            lifetime = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _rawLifetime.Token);
        }
        using (lifetime)
        {
            var token = lifetime.Token;
            token.ThrowIfCancellationRequested();
            var pending = waitForResponse ? new PendingPacket(true) : null;
            if (pending is not null && !_pendingTasks.TryAdd(packet.Sequence, pending))
                throw new InvalidOperationException($"Packet sequence {packet.Sequence} is already pending.");
            try
            {
                await SendFrame(packet, new ServiceAttribute(packet.Command, RequestType.D2Auth, EncryptType.EncryptD2Key), token);
                return pending is null ? null : await pending.Completion.Task.WaitAsync(token);
            }
            catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                throw new IOException("The connection closed while sending a raw packet.", ex);
            }
            finally
            {
                if (pending is not null)
                    _pendingTasks.TryRemove(new KeyValuePair<int, PendingPacket>(packet.Sequence, pending));
            }
        }
    }

    private async ValueTask SendFrame(BotSsoPacket packet, ServiceAttribute options, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        LogPacket(packet, options);
        var secInfo = SignProvider.IsWhiteListCommand(packet.Command)
            ? await SignProvider.GetSecSign(_keystore.Uin, packet.Command, packet.Sequence, packet.Data).WaitAsync(token)
            : null;
        token.ThrowIfCancellationRequested();
        ReadOnlyMemory<byte> frame;
        switch (options.RequestType)
        {
            case RequestType.D2Auth:
                frame = _servicePacker.BuildProtocol12(_ssoPacker.BuildProtocol12(packet, secInfo), options);
                break;
            case RequestType.Simple:
                using (var sso = _ssoPacker.BuildProtocol13(packet, secInfo))
                    frame = _servicePacker.BuildProtocol13(packet, sso, options);
                break;
            default:
                throw new InvalidOperationException($"Unknown RequestType: {options.RequestType}");
        }
        token.ThrowIfCancellationRequested();
        await _send(frame, token);
    }

    internal int PendingCount => _pendingTasks.Count;

    public void OnDisconnect()
    {
        CancellationTokenSource previous;
        lock (_lifetimeLock)
        {
            if (_disposed) return;
            previous = _rawLifetime;
            _rawLifetime = new();
        }
        previous.Cancel();
        previous.Dispose();
    }

    public void Dispose()
    {
        lock (_lifetimeLock)
        {
            if (_disposed) return;
            _disposed = true;
        }
        _rawLifetime.Cancel();
        _rawLifetime.Dispose();
    }

    public void DispatchPacket(ReadOnlySpan<byte> buffer)
    {
        var service = _servicePacker.Parse(buffer);
        var sso = _ssoPacker.Parse(service);
        DispatchSsoPacket(sso);
    }

    private void DispatchSsoPacket(BotSsoPacket sso)
    {
        if (!_context.Config.RawPacketCommandBlacklist.Contains(sso.Command))
        {
            // PostEvent schedules handlers independently; never wait for event delivery on the receive path.
            if (_context.EventInvoker.HasSubscribers<BotPacketReceivedEvent>())
                _context.EventInvoker.PostEvent(new BotPacketReceivedEvent(sso));
            LogPacket(sso);
        }
        if (_pendingTasks.TryRemove(sso.Sequence, out var pending))
        {
            if (!pending.Raw && sso is { RetCode: not 0, Extra: var extra })
            {
                string msg = $"Packet '{sso.Command}' returns {sso.RetCode} with seq: {sso.Sequence}, extra: {extra}";
                pending.Completion.TrySetException(new InvalidOperationException(msg));
            }
            else
            {
                pending.Completion.TrySetResult(sso);
            }
        }
        else
        {
            Task.Run(() => _context.EventContext.HandleServerPacket(sso));
        }
    }

    private void LogPacket(BotSsoPacket packet, ServiceAttribute? options = null)
    {
        if (_context.Config.LogLevel > LogLevel.Trace ||
            _context.Config.RawPacketCommandBlacklist.Contains(packet.Command) ||
            !_context.EventInvoker.HasSubscribers<BotLogEvent>()) return;

        // Publish as the existing base event so ordinary Core log subscribers
        // retain their readable message, while adapters can inspect packet fields.
        _context.EventInvoker.PostEvent<BotLogEvent>(BotPacketLogEvent.Create(packet, options));
    }
}
