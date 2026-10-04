using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Channels;
using System.Net.WebSockets;
using Lagrange.Core;
using Lagrange.Core.Events;
using Lagrange.Milky.Configurations;
using Lagrange.Milky.Events.Converters;
using Lagrange.Milky.Events.Extensions;
using Lagrange.Milky.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace Lagrange.Milky.Events;

public sealed class MilkyEventHub(
    IServiceScopeFactory scopes,
    BotContext bot,
    MilkyConfiguration configuration,
    ILogger<MilkyEventHub> logger) : IHostedService, IGenericEventHandler
{
    private readonly ConcurrentDictionary<Guid, Channel<byte[]>> _clients = new();
    private readonly CancellationTokenSource _stop = new();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        bot.RegisterConvertibleEvents(this);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _stop.Cancel();
        bot.UnregisterConvertibleEvents(this);
        foreach (var channel in _clients.Values) channel.Writer.TryComplete();
        return Task.CompletedTask;
    }

    public EventSubscription Subscribe(int capacity = 64)
    {
        var id = Guid.NewGuid();
        var channel = Channel.CreateBounded<byte[]>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.DropWrite,
            SingleReader = true,
            SingleWriter = false
        });
        _clients[id] = channel;
        return new(id, channel.Reader, () =>
        {
            if (_clients.TryRemove(id, out var removed))
            {
                removed.Writer.TryComplete();
            }
        });
    }

    public async Task OnEvent<TEvent>(BotContext context, TEvent @event)
        where TEvent : Lagrange.Core.Events.EventBase
    {
        if (configuration.Event.SuppressSelfMessageEvents && @event is Lagrange.Core.Events.EventArgs.BotMessageEvent message && message.Message.Contact.Uin == bot.BotUin)
            return;

        await using var scope = scopes.CreateAsyncScope();
        var converter = scope.ServiceProvider.GetServices<IEventConverter<TEvent>>().FirstOrDefault(x => x.CanConvert(@event));
        if (converter is null)
        {
            return;
        }
        object data = await converter.ConvertAsync(@event, _stop.Token);
        byte[] payload = Serializer.JsonSerializeToUtf8Bytes(new MilkyEvent
        {
            EventType = converter.Name,
            Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            SelfId = bot.BotUin,
            Data = data
        });
        foreach (var pair in _clients)
            if (!pair.Value.Writer.TryWrite(payload))
                logger.LogWarning("Dropping event for slow client {ClientId}", pair.Key);
    }
}

public sealed class EventSubscription(Guid id, ChannelReader<byte[]> reader, Action dispose) : IDisposable
{
    public Guid Id { get; } = id;
    public ChannelReader<byte[]> Reader { get; } = reader;
    public void Dispose() => dispose();
}

public sealed class MilkySseEndpoint(MilkyEventHub hub, MilkyConfiguration configuration) : FastEndpoints.EndpointWithoutRequest
{
    public override void Configure()
    {
        AuthSchemes("Milky");
        Get("/event");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!Auth(HttpContext.Request, configuration.AccessToken))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        using var subscription = hub.Subscribe();
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var socket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            var receive = ReceiveUntilClosed(socket, ct);
            while (await subscription.Reader.WaitToReadAsync(ct))
                while (subscription.Reader.TryRead(out byte[]? payload))
                    await socket.SendAsync(payload, WebSocketMessageType.Text, true, ct);
            await receive;
            return;
        }
        await Send.EventStreamAsync("milky_event", Stream(subscription, configuration.Event.SSE?.HeartbeatIntervalSeconds ?? 60, ct), ct);
    }

    private static async Task ReceiveUntilClosed(WebSocket socket, CancellationToken ct)
    {
        byte[] buffer = new byte[1024];
        while (socket.State == WebSocketState.Open)
        {
            var result = await socket.ReceiveAsync(buffer, ct);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, ct);
                return;
            }
        }
    }

    private static async IAsyncEnumerable<object> Stream(EventSubscription subscription, ulong heartbeat, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(Math.Max(1, heartbeat)));
        while (!ct.IsCancellationRequested)
        {
            var read = subscription.Reader.WaitToReadAsync(ct).AsTask();
            var tick = timer.WaitForNextTickAsync(ct).AsTask();
            if (await Task.WhenAny(read, tick) == tick) { yield return new { }; continue; }
            if (!await read) yield break;
            while (subscription.Reader.TryRead(out byte[]? bytes))
                yield return JsonDocument.Parse(bytes).RootElement.Clone();
        }
    }

    internal static bool Auth(HttpRequest request, string? token)
        => string.IsNullOrEmpty(token) || request.Headers.Authorization == $"Bearer {token}" || request.Query["access_token"] == token;
}
