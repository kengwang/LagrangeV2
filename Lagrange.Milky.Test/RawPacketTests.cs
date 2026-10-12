using System.Buffers.Binary;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using FastEndpoints;
using Lagrange.Core;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Utility.Binary;
using Lagrange.Core.Utility.Cryptography;
using Lagrange.Milky.Api.Handlers.System;
using Lagrange.Milky.Configurations;
using Lagrange.Milky.Events;
using Lagrange.Milky.Events.Converters;
using Lagrange.Milky.Events.Extensions;
using Lagrange.Milky.Extensions;
using Lagrange.Milky.Security;
using Lagrange.Milky.Serialization;
using Lagrange_Milky;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lagrange.Milky.Test;

public sealed class RawPacketTests
{
    private static BotContext Bot() => BotFactory.Create(new BotConfig { GetOptimumServer = false }, new BotKeystore(), BotAppInfo.ProtocolToAppInfo[Protocols.Linux]);

    [Test]
    public void RequestDefaultsAndExplicitNullsUseGeneratedMetadata()
    {
        var options = new JsonSerializerOptions().AddSerializerContextsFromLagrange_Milky();
        var info = options.GetTypeInfo(typeof(SendPacketHandler.Request));
        var defaults = (SendPacketHandler.Request)JsonSerializer.Deserialize("""{"cmd":"Test.Raw"}""", info)!;
        var nulls = (SendPacketHandler.Request)JsonSerializer.Deserialize("""{"cmd":null,"data":null,"rsp":null}""", info)!;
        Assert.That(defaults.Data, Is.Empty);
        Assert.That(defaults.WaitForResponse, Is.True);
        Assert.That(nulls.Command, Is.Null);
        Assert.That(nulls.Data, Is.Null);
        Assert.That(nulls.WaitForResponse, Is.Null);
    }

    [Test]
    public void ConfigurationDefaultsAndExplicitOptInBindCorrectly()
    {
        foreach (string? value in new string?[] { null, "false", "true" })
        {
            var values = new Dictionary<string, string?>
            {
                ["Milky:HttpServer:Host"] = "127.0.0.1",
                ["Milky:HttpServer:Port"] = "3000",
                ["Milky:Event:EnableRawPacketEvents"] = value
            };
            if (value is null) values.Remove("Milky:Event:EnableRawPacketEvents");
            // Exercise the production source-generated binder, which can bind Event_.
            var builder = WebApplication.CreateBuilder();
            builder.Configuration.AddInMemoryCollection(values);
            builder.ConfigureMilky();
            using var services = builder.Services.BuildServiceProvider();
            var settings = services.GetRequiredService<MilkyConfiguration>();
            Assert.That(settings.Event.EnableRawPacketEvents, Is.EqualTo(value == "true"));
        }
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task HubOnlySubscribesWhenExplicitlyEnabledAndUnsubscribesOnStop(bool enabled)
    {
        using var bot = Bot();
        var services = new ServiceCollection();
        services.AddEventConverters();
        services.AddSingleton<IEventConverter<BotPacketReceivedEvent>, PacketReceivedEventConverter>();
        using var provider = services.BuildServiceProvider();
        var hub = new MilkyEventHub(provider.GetRequiredService<IServiceScopeFactory>(), bot,
            new MilkyConfiguration(new("127.0.0.1", 0), event_: new(enableRawPacketEvents: enabled)), NullLogger<MilkyEventHub>.Instance);
        await hub.StartAsync(default);
        using var subscription = hub.Subscribe();
        Assert.That(bot.EventInvoker.HasSubscribers<BotPacketReceivedEvent>(), Is.EqualTo(enabled));
        var packet = new BotSsoPacket("Test.Raw", new byte[] { 0xAB, 0 }, 7, 8, "error");
        var received = new BotPacketReceivedEvent(packet);
        await hub.OnEvent(bot, received);
        if (enabled)
        {
            using var json = JsonDocument.Parse(await subscription.Reader.ReadAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(3)));
            Assert.That(json.RootElement.GetProperty("event_type").GetString(), Is.EqualTo("packet_received"));
            Assert.That(json.RootElement.GetProperty("time").GetInt64(), Is.EqualTo(received.EventTime));
            Assert.That(json.RootElement.GetProperty("data").GetProperty("data").GetString(), Is.EqualTo("AB00"));
            Assert.That(json.RootElement.GetProperty("data").GetProperty("ret_code").GetInt32(), Is.EqualTo(8));
        }
        else Assert.That(subscription.Reader.TryRead(out _), Is.False);
        await hub.StopAsync(default);
        Assert.That(bot.EventInvoker.HasSubscribers<BotPacketReceivedEvent>(), Is.False);
        Assert.That(subscription.Reader.TryRead(out _), Is.False);
    }

    [Test]
    public async Task HttpContractValidatesBeforeSendingAndReturnsRawProtocolErrors()
    {
        using var bot = Bot();
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Services.AddSingleton(bot);
        builder.Services.AddSingleton(new MilkyConfiguration(new("127.0.0.1", 0), "test-token"));
        builder.Services.AddAuthentication("Milky").AddScheme<AuthenticationSchemeOptions, MilkyAuthenticationHandler>("Milky", _ => { });
        builder.Services.AddAuthorization();
        builder.Services.AddFastEndpoints(options =>
        {
            options.DisableAutoDiscovery = true;
            options.Assemblies = [typeof(SendPacketHandler).Assembly];
            options.Filter = type => type == typeof(SendPacketHandler);
        });
        await using var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseFastEndpoints(options => options.ConfigureMilkyBinding());
        await app.StartAsync();
        using var client = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };
        try
        {
            using var unauthorized = await client.PostAsync("/api/send_packet", Body("{}"));
            Assert.That(unauthorized.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");
            string[] invalid = ["{}", """{"cmd":" "}""", """{"cmd":null}""", """{"cmd":"Test.Raw","data":null}""",
                """{"cmd":"Test.Raw","rsp":null}""", """{"cmd":"Test.Raw","data":"0"}""",
                """{"cmd":"Test.Raw","data":"GG"}""", """{"cmd":"Test.Raw","data":"08 01"}"""];
            foreach (string body in invalid)
            {
                using var json = await Post(client, body);
                Assert.That(json.RootElement.GetProperty("retcode").GetInt64(), Is.EqualTo(-400), body);
                Assert.That(bot.PacketContext.PendingCount, Is.Zero);
            }
            using (var offline = await Post(client, """{"cmd":"Test.Raw","rsp":false}"""))
                Assert.That(offline.RootElement.GetProperty("retcode").GetInt64(), Is.EqualTo(-500));

            using var server = new TcpListener(IPAddress.Loopback, 0);
            server.Start();
            var accepted = server.AcceptTcpClientAsync();
            bool connected = await bot.SocketContext.Connect("127.0.0.1", (ushort)((IPEndPoint)server.LocalEndpoint).Port);
            Assert.That(connected, Is.True);
            using var peer = await accepted;
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var stream = peer.GetStream();
            var send = Post(client, """{"cmd":"Test.Raw","data":"0801aB00"}""");
            var request = await ReadRequest(stream, timeout.Token);
            Assert.That(request.Data.ToArray(), Is.EqualTo(new byte[] { 8, 1, 0xAB, 0 }));
            await stream.WriteAsync(Response(request.Sequence, 7), timeout.Token);
            using (var response = await send)
            {
                Assert.That(response.RootElement.GetProperty("status").GetString(), Is.EqualTo("ok"));
                var data = response.RootElement.GetProperty("data");
                Assert.That(data.GetProperty("cmd").GetString(), Is.EqualTo("Test.Raw"));
                Assert.That(data.GetProperty("data").GetString(), Is.EqualTo("0801AB00"));
                Assert.That(data.GetProperty("sequence").GetInt32(), Is.EqualTo(request.Sequence));
                Assert.That(data.GetProperty("ret_code").GetInt32(), Is.EqualTo(7));
                Assert.That(data.GetProperty("extra").GetString(), Is.EqualTo("error"));
            }
            var noResponse = Post(client, """{"cmd":"Test.Raw","rsp":false}""");
            var empty = await ReadRequest(stream, timeout.Token);
            Assert.That(empty.Data.IsEmpty, Is.True);
            Assert.That(empty.Sequence, Is.Not.EqualTo(request.Sequence));
            using (var response = await noResponse)
            {
                Assert.That(response.RootElement.GetProperty("status").GetString(), Is.EqualTo("ok"));
                Assert.That(response.RootElement.GetProperty("data").EnumerateObject().Count(), Is.Zero);
            }
            Assert.That(bot.PacketContext.PendingCount, Is.Zero);
            var timed = Post(client, """{"cmd":"Test.Raw"}""");
            await ReadRequest(stream, timeout.Token);
            using (var response = await timed.WaitAsync(TimeSpan.FromSeconds(15)))
                Assert.That(response.RootElement.GetProperty("retcode").GetInt64(), Is.EqualTo(-408));
            Assert.That(bot.PacketContext.PendingCount, Is.Zero);

            using var aborted = new CancellationTokenSource();
            var canceledRequest = client.PostAsync("/api/send_packet", Body("""{"cmd":"Test.Raw"}"""), aborted.Token);
            using var cleanup = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            await ReadRequest(stream, cleanup.Token);
            aborted.Cancel();
            Assert.That(async () => await canceledRequest, Throws.InstanceOf<OperationCanceledException>());
            while (bot.PacketContext.PendingCount > 0) await Task.Delay(10, cleanup.Token);

            var disconnected = Post(client, """{"cmd":"Test.Raw"}""");
            await ReadRequest(stream, cleanup.Token);
            peer.Close();
            using (var response = await disconnected.WaitAsync(TimeSpan.FromSeconds(3)))
                Assert.That(response.RootElement.GetProperty("retcode").GetInt64(), Is.EqualTo(-500));
            Assert.That(bot.PacketContext.PendingCount, Is.Zero);
        }
        finally { await app.StopAsync(); }
    }

    private static StringContent Body(string body) => new(body, Encoding.UTF8, "application/json");

    [Test]
    public async Task WebSocketSseAndWebhookReceiveTheSameRawEvents()
    {
        using var bot = Bot();
        var webhookEvents = System.Threading.Channels.Channel.CreateUnbounded<byte[]>();
        var receiverBuilder = WebApplication.CreateBuilder();
        receiverBuilder.WebHost.UseUrls("http://127.0.0.1:0");
        receiverBuilder.Logging.ClearProviders();
        await using var receiver = receiverBuilder.Build();
        receiver.MapPost("/hook", async (Microsoft.AspNetCore.Http.HttpContext context) =>
        {
            Assert.That(context.Request.Headers.Authorization.ToString(), Is.EqualTo("Bearer test-token"));
            using var content = new MemoryStream();
            await context.Request.Body.CopyToAsync(content);
            await webhookEvents.Writer.WriteAsync(content.ToArray());
            return Microsoft.AspNetCore.Http.Results.Ok();
        });
        await receiver.StartAsync();

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Services.AddSingleton(bot);
        builder.Services.AddSingleton(new MilkyConfiguration(new("127.0.0.1", 0), "test-token",
            event_: new(webSocket: new(true), sse: new(true), webHook: new(true, [receiver.Urls.Single() + "/hook"]), enableRawPacketEvents: true)));
        builder.Services.AddSingleton<IEventConverter<BotPacketReceivedEvent>, PacketReceivedEventConverter>();
        builder.Services.AddSingleton<MilkyEventHub>();
        builder.Services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService>(sp => sp.GetRequiredService<MilkyEventHub>());
        builder.Services.AddHttpClient(nameof(WebHookEventHandler));
        builder.Services.AddHostedService<WebHookEventHandler>();
        builder.Services.AddAuthentication("Milky").AddScheme<AuthenticationSchemeOptions, MilkyAuthenticationHandler>("Milky", _ => { });
        builder.Services.AddAuthorization();
        builder.Services.AddFastEndpoints(options =>
        {
            options.DisableAutoDiscovery = true;
            options.Assemblies = [typeof(MilkySseEndpoint).Assembly];
            options.Filter = type => type == typeof(MilkySseEndpoint);
        });
        await using var app = builder.Build();
        app.UseWebSockets();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseFastEndpoints(options => options.ConfigureMilkyBinding());
        await app.StartAsync();
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var hub = app.Services.GetRequiredService<MilkyEventHub>();
        using var socket = new ClientWebSocket();
        socket.Options.SetRequestHeader("Authorization", "Bearer test-token");
        using var client = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");
        using var sseRequest = new HttpRequestMessage(HttpMethod.Get, "/event");
        try
        {
            await socket.ConnectAsync(new Uri(app.Urls.Single().Replace("http://", "ws://") + "/event"), deadline.Token);
            var sseResponseTask = client.SendAsync(sseRequest, HttpCompletionOption.ResponseHeadersRead, deadline.Token);
            while (hub.SubscriptionCount < 3) await Task.Delay(10, deadline.Token);
            var first = new BotPacketReceivedEvent(new BotSsoPacket("Test.Raw", new byte[] { 0xAB, 0 }, 81, 7, "error"));
            bot.EventInvoker.PostEvent(first);
            using var sseResponse = await sseResponseTask;
            using var sseReader = new StreamReader(await sseResponse.Content.ReadAsStreamAsync(deadline.Token));
            await CompareEvent(first);
            // A second event exercises the still-pending SSE heartbeat wait.
            var second = new BotPacketReceivedEvent(new BotSsoPacket("Test.Raw", new byte[] { 8, 1 }, 82, 0, ""));
            bot.EventInvoker.PostEvent(second);
            await CompareEvent(second);

            async Task CompareEvent(BotPacketReceivedEvent packet)
            {
                var buffer = new byte[4096];
                var ws = await socket.ReceiveAsync(buffer, deadline.Token);
                Assert.That(ws.EndOfMessage, Is.True);
                using var websocket = JsonDocument.Parse(buffer.AsMemory(0, ws.Count));
                string? line;
                do { line = await sseReader.ReadLineAsync(deadline.Token); }
                while (line is not null && !line.StartsWith("data:"));
                Assert.That(line, Is.Not.Null);
                using var sse = JsonDocument.Parse(line![5..].Trim());
                using var webhook = JsonDocument.Parse(await webhookEvents.Reader.ReadAsync(deadline.Token));
                Assert.That(websocket.RootElement.GetRawText(), Is.EqualTo(webhook.RootElement.GetRawText()));
                Assert.That(sse.RootElement.GetRawText(), Is.EqualTo(webhook.RootElement.GetRawText()));
                Assert.That(webhook.RootElement.GetProperty("time").GetInt64(), Is.EqualTo(packet.EventTime));
                Assert.That(webhook.RootElement.GetProperty("data").GetProperty("sequence").GetInt32(), Is.EqualTo(packet.Sequence));
            }
        }
        finally
        {
            deadline.Cancel();
            socket.Abort();
            using var stop = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            await app.StopAsync(stop.Token);
            await receiver.StopAsync();
        }
    }

    private static async Task<JsonDocument> Post(HttpClient client, string body)
    {
        using var response = await client.PostAsync("/api/send_packet", Body(body));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    }

    private static async Task<BotSsoPacket> ReadRequest(NetworkStream stream, CancellationToken ct)
    {
        byte[] header = new byte[4];
        await stream.ReadExactlyAsync(header, ct);
        byte[] frame = new byte[BinaryPrimitives.ReadInt32BigEndian(header) - 4];
        await stream.ReadExactlyAsync(frame, ct);
        return DecodeRequest(frame);
    }

    private static BotSsoPacket DecodeRequest(byte[] frame)
    {
        var reader = new BinaryPacket(frame);
        Assert.That(reader.Read<int>(), Is.EqualTo(12));
        Assert.That(reader.Read<byte>(), Is.EqualTo((byte)EncryptType.EncryptD2Key));
        reader.ReadBytes(Prefix.Int32 | Prefix.WithPrefix); // D2
        reader.Read<byte>();
        reader.ReadString(Prefix.Int32 | Prefix.WithPrefix); // UIN
        byte[] decrypted = TeaProvider.Decrypt(reader.ReadBytes(), new byte[16]);
        var sso = new BinaryPacket(decrypted);
        var head = new BinaryPacket(sso.ReadBytes(Prefix.Int32 | Prefix.WithPrefix));
        int sequence = head.Read<int>();
        var payload = sso.ReadBytes(Prefix.Int32 | Prefix.WithPrefix).ToArray();
        return new BotSsoPacket("Test.Raw", payload, sequence);
    }

    private static byte[] Response(int sequence, int retCode)
    {
        using var head = new BinaryPacket(256);
        head.Write(sequence);
        head.Write(retCode);
        head.Write("error", Prefix.Int32 | Prefix.WithPrefix);
        head.Write("Test.Raw", Prefix.Int32 | Prefix.WithPrefix);
        head.Write(ReadOnlySpan<byte>.Empty, Prefix.Int32 | Prefix.WithPrefix);
        head.Write(0);
        head.Write(ReadOnlySpan<byte>.Empty, Prefix.Int32 | Prefix.WithPrefix);
        using var sso = new BinaryPacket(512);
        sso.Write(head.CreateReadOnlySpan(), Prefix.Int32 | Prefix.WithPrefix);
        sso.Write(new byte[] { 8, 1, 0xAB, 0 }, Prefix.Int32 | Prefix.WithPrefix);
        using var frame = new BinaryPacket(1024);
        frame.EnterLengthBarrier<int>();
        frame.Write(12);
        frame.Write((byte)EncryptType.NoEncrypt);
        frame.Write((byte)0);
        frame.Write("123", Prefix.Int32 | Prefix.WithPrefix);
        frame.Write(sso.CreateReadOnlySpan());
        frame.ExitLengthBarrier<int>(true);
        return frame.CreateReadOnlySpan().ToArray();
    }
}
